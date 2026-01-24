using FiscalDocumentationService.Business.Interfaces;
using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Interfaces.Clients.Dummy;
using FiscalDocumentationService.Business.Models;
using FiscalDocumentationService.Business.Models.Arca;
using FiscalDocumentationService.Business.Options;
using FiscalDocumentationService.Business.Services.Clients;
using FiscalDocumentationService.Infrastructure.Interfaces;
using FiscalDocumentationService.Infrastructure.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static FiscalDocumentationService.Business.Exceptions.FiscalDocumentationException;

namespace FiscalDocumentationService.Business.Services
{
    public class FiscalDocumentService : IFiscalDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDummyArcaServiceClient _dummyArcaClient;
        private readonly ICompanyServiceClient _companyClient;
        private readonly IOptions<ArcaOptions> _arcaOptions;
        private readonly IArcaWsfeClient _arcaWsfeClient;

        public FiscalDocumentService(IUnitOfWork unitOfWork, IDummyArcaServiceClient dummyArcaClient, ICompanyServiceClient companyClient, 
            IOptions<ArcaOptions> arcaOptions, IArcaWsfeClient arcaWsfeClient)
        {
            _unitOfWork = unitOfWork;
            _dummyArcaClient = dummyArcaClient;
            _companyClient = companyClient;
            _arcaOptions = arcaOptions;
            _arcaWsfeClient = arcaWsfeClient;
        }

        public async Task<FiscalDocumentDTO> CreateAsync(FiscalDocumentCreateDTO dto)
        {
            // Step 1: Retrieve company fiscal settings to determine ARCA configuration
            var companyFiscal = await _companyClient.GetCompanyFiscalSettingsAsync();
            if (companyFiscal == null)
                throw new FiscalConfigurationException("Company fiscal settings not found.");

            if (companyFiscal.ArcaEnabled && companyFiscal.ArcaPointOfSale == null)
                throw new FiscalConfigurationException("ARCA is enabled but PointOfSale is not configured in CompanyInfo.");

            // Step 2: Validate environment consistency between local config and company service
            var arcaEnv = (_arcaOptions.Value.Environment ?? "Homologation").Trim();
            if (!string.IsNullOrWhiteSpace(companyFiscal.ArcaEnvironment) &&
                !companyFiscal.ArcaEnvironment.Equals(arcaEnv, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"ARCA environment mismatch. Config='{arcaEnv}' CompanyService='{companyFiscal.ArcaEnvironment}'.");
            }

            // Step 3: Validate document items and totals
            if (dto.Items == null || dto.Items.Count == 0)
                throw new FiscalValidationException("At least one item is required.");

            if (dto.TotalAmount <= 0)
                throw new FiscalValidationException("TotalAmount must be greater than 0.");

            if (dto.BuyerDocumentNumber <= 0)
                throw new FiscalValidationException("BuyerDocumentNumber must be greater than 0.");

            ValidateBuyerDataByInvoiceType(dto);

            // Verify total amount matches sum of components
            var calcTotal = dto.NetAmount + dto.VatAmount + dto.ExemptAmount + dto.NonTaxableAmount + dto.OtherTaxesAmount;
            if (Math.Abs(calcTotal - dto.TotalAmount) > 0.01m)
                throw new ArgumentException($"TotalAmount mismatch. Expected {calcTotal} but got {dto.TotalAmount}.");

            // Step 4: Map invoice type code to document type enum
            var type = dto.InvoiceType switch
            {
                1 => FiscalDocumentType.Invoice,
                6 => FiscalDocumentType.Invoice,
                11 => FiscalDocumentType.Invoice,
                51 => FiscalDocumentType.Invoice,
                2 => FiscalDocumentType.DebitNote,
                7 => FiscalDocumentType.DebitNote,
                12 => FiscalDocumentType.DebitNote,
                52 => FiscalDocumentType.DebitNote,
                3 => FiscalDocumentType.CreditNote,
                8 => FiscalDocumentType.CreditNote,
                13 => FiscalDocumentType.CreditNote,
                53 => FiscalDocumentType.CreditNote,
                _ => throw new ArgumentException("Invalid Invoice Type")
            };

            // Step 5: Validate referenced document fields for credit/debit notes
            if (type == FiscalDocumentType.CreditNote || type == FiscalDocumentType.DebitNote)
            {
                if (!dto.ReferencedInvoiceType.HasValue)
                    throw new FiscalValidationException($"{type} requires ReferencedInvoiceType (tipo de comprobante referenciado).");
                if (!dto.ReferencedPointOfSale.HasValue)
                    throw new FiscalValidationException($"{type} requires ReferencedPointOfSale (punto de venta del comprobante referenciado).");
                if (!dto.ReferencedInvoiceNumber.HasValue || dto.ReferencedInvoiceNumber <= 0)
                    throw new FiscalValidationException($"{type} requires ReferencedInvoiceNumber (número del comprobante referenciado).");
            }

            // Step 6: Implement idempotency for invoices - return existing if already present
            if (type == FiscalDocumentType.Invoice)
            {
                var existingInvoices = await _unitOfWork.FiscalDocumentRepository
                    .GetBySaleIdIdAsync(dto.SalesOrderId, FiscalDocumentType.Invoice);

                var existing = existingInvoices
                    .OrderByDescending(d => d.Date)
                    .FirstOrDefault();

                if (existing != null)
                    return MapToDTO(existing);
            }

            // Step 7: Extract and validate issuer CUIT (remove non-numeric characters)
            var issuerCuitDigits = Regex.Replace(companyFiscal.TaxId ?? "", @"\D", "");
            if (!long.TryParse(issuerCuitDigits, out var issuerCuit))
                throw new FiscalConfigurationException($"Invalid Company TaxId format: '{companyFiscal.TaxId}'");

            // Step 8: Create document entity in Pending status (audit-friendly)
            var document = new FiscalDocument
            {
                PointOfSale = companyFiscal.ArcaPointOfSale ?? 1,

                InvoiceType = dto.InvoiceType,
                BuyerDocumentType = dto.BuyerDocumentType,
                BuyerDocumentNumber = dto.BuyerDocumentNumber,

                // Invoice numbers will be assigned later (by dummy or WSFE)
                InvoiceFrom = 0,
                InvoiceTo = 0,

                Date = DateTime.Now,
                Type = type,

                NetAmount = dto.NetAmount,
                VATAmount = dto.VatAmount,
                ExemptAmount = dto.ExemptAmount,
                NonTaxableAmount = dto.NonTaxableAmount,
                OtherTaxesAmount = dto.OtherTaxesAmount,
                TotalAmount = dto.TotalAmount,

                SalesOrderId = dto.SalesOrderId,

                Currency = dto.Currency,
                ExchangeRate = dto.ExchangeRate,
                IssuerTaxId = issuerCuitDigits,

                ReceiverVatConditionId = dto.ReceiverVatConditionId,

                // Reference fields for credit/debit notes
                ReferencedInvoiceType = dto.ReferencedInvoiceType,
                ReferencedPointOfSale = dto.ReferencedPointOfSale,
                ReferencedInvoiceNumber = dto.ReferencedInvoiceNumber,

                Items = dto.Items.Select(i => new FiscalDocumentItem
                {
                    Sku = i.Sku,
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    VATId = i.VatId,
                    VATBase = i.VatBase,
                    VATAmount = i.VatAmount,
                    DispatchCode = i.DispatchCode,
                    Warranty = i.Warranty
                }).ToList(),

                EmissionProvider = companyFiscal.ArcaEnabled ? "WSFE" : "Dummy",
                ArcaEnvironment = arcaEnv,

                ArcaStatus = "Pending",
                ArcaLastInteractionAt = DateTime.UtcNow,
                ArcaCorrelationId = Guid.NewGuid()
            };

            await _unitOfWork.FiscalDocumentRepository.CreateAsync(document);
            await _unitOfWork.SaveChangesAsync();

            // Step 9: Process document through ARCA (Dummy or Real WSFE)
            if (!companyFiscal.ArcaEnabled)
            {
                // Dummy flow: simulates ARCA authorization without actual contacting ARCA
                var invoiceNumber = GenerateInvoiceNumber();
                document.InvoiceFrom = invoiceNumber;
                document.InvoiceTo = invoiceNumber;

                var arcaRequest = BuildArcaRequest(document);

                document.ArcaRequestJson = JsonSerializer.Serialize(arcaRequest);
                document.ArcaLastInteractionAt = DateTime.UtcNow;

                var arcaResponse = await _dummyArcaClient.AuthorizeAsync(arcaRequest);

                document.ArcaResponseJson = JsonSerializer.Serialize(arcaResponse);

                document.CAE = arcaResponse.cae;
                document.CAEExpiration = DateTime.ParseExact(arcaResponse.caeExpirationDate, "yyyyMMdd", null);
                document.DocumentNumber = $"{document.PointOfSale:0000}-{document.InvoiceFrom:00000000}";
                document.ArcaStatus = "Authorized";
            }
            else
            {
                // Real WSFE flow: contact actual ARCA service
                document.ArcaLastInteractionAt = DateTime.UtcNow;

                // 9.1) Get next available invoice number from WSFE
                var last = await _arcaWsfeClient.GetLastAuthorizedAsync(
                    issuerCuit,
                    document.PointOfSale,
                    document.InvoiceType);

                var next = last + 1;
                document.InvoiceFrom = next;
                document.InvoiceTo = next;
                document.DocumentNumber = $"{document.PointOfSale:0000}-{next:00000000}";

                // 9.2) Build WSFE CAE request
                var wsfeReq = BuildWsfeCaeRequest(document);

                document.ArcaRequestJson = JsonSerializer.Serialize(wsfeReq);

                // 9.3) Submit CAE request to WSFE
                var caeResp = await _arcaWsfeClient.RequestCaeAsync(issuerCuit, wsfeReq);

                document.ArcaResponseJson = JsonSerializer.Serialize(caeResp);

                // 9.4) Process WSFE response
                document.ArcaLastInteractionAt = DateTime.UtcNow;

                if (caeResp.Errors != null && caeResp.Errors.Count > 0)
                    document.ArcaErrorsJson = JsonSerializer.Serialize(caeResp.Errors);

                if (caeResp.Events != null && caeResp.Events.Count > 0)
                    document.ArcaObservationsJson = JsonSerializer.Serialize(caeResp.Events);

                // Check if ARCA approved or rejected the document
                if (!string.Equals(caeResp.Result, "A", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrWhiteSpace(caeResp.Cae))
                {
                    document.ArcaStatus = "Rejected";
                    await _unitOfWork.SaveChangesAsync();

                    throw new FiscalValidationException(
                        "ARCA rejected the document: " +
                        string.Join(" | ", (caeResp.Errors ?? new()).Select(e => $"{e.Code}: {e.Msg}"))
                    );
                }

                // Document was approved (Result = "A")
                if (string.Equals(caeResp.Result, "A", StringComparison.OrdinalIgnoreCase))
                {
                    document.ArcaStatus = "Authorized";

                    document.CAE = caeResp.Cae ?? "";
                    if (!string.IsNullOrWhiteSpace(caeResp.CaeDueDate))
                    {
                        document.CAEExpiration = DateTime.ParseExact(caeResp.CaeDueDate, "yyyyMMdd", null);
                    }
                }
                else
                {
                    // Other result codes (Rejected, etc.)
                    document.ArcaStatus = "Rejected";
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return MapToDTO(document);
        }



        /// <summary>
        /// Builds a WSFE (Web Service de Facturación Electrónica) CAE request from a FiscalDocument.
        /// This request is sent to ARCA to obtain authorization (CAE - Código de Autorización Electrónica).
        /// </summary>
        private WsfeCaeRequest BuildWsfeCaeRequest(FiscalDocument doc)
        {
            // Concept code: 1 = Products (fixed for this company)
            const int concept = 1;

            // Convert date to DateOnly (WSFE requirement)
            var cbteDate = DateOnly.FromDateTime(doc.Date);

            // Group VAT items by rate (VATId) - WSFE requires aggregated amounts by rate
            // This prevents duplicates and ensures correct structure
            var vatItems = doc.Items
                .GroupBy(i => i.VATId)
                .Select(g => new WsfeVatItem(
                    VatId: g.Key,
                    BaseAmount: g.Sum(x => x.VATBase),
                    VatAmount: g.Sum(x => x.VATAmount)
                ))
                .ToList();

            // WSFE validation rule: if NetAmount > 0, VatItems must exist and sum correctly
            // If NetAmount == 0, send empty list to avoid unnecessary IVA nodes
            if (doc.NetAmount <= 0)
                vatItems = new List<WsfeVatItem>();

            return new WsfeCaeRequest(
                PointOfSale: doc.PointOfSale,
                CbteType: doc.InvoiceType,
                Concept: concept,
                DocType: doc.BuyerDocumentType,
                DocNumber: doc.BuyerDocumentNumber,
                CbteDate: cbteDate,
                CbteFrom: doc.InvoiceFrom,
                CbteTo: doc.InvoiceTo,
                CurrencyId: doc.Currency,
                CurrencyRate: doc.ExchangeRate,
                TotalAmount: doc.TotalAmount,
                NetAmount: doc.NetAmount,
                VatAmount: doc.VATAmount,
                NotTaxedAmount: doc.NonTaxableAmount,
                ExemptAmount: doc.ExemptAmount,
                OtherTaxesAmount: doc.OtherTaxesAmount,
                VatItems: vatItems,
                ReceiverVatConditionId: doc.ReceiverVatConditionId,
                // Reference fields for credit/debit notes
                ReferencedCbteType: doc.ReferencedInvoiceType,
                ReferencedPointOfSale: doc.ReferencedPointOfSale,
                ReferencedCbteNumber: doc.ReferencedInvoiceNumber
            );
        }

        /// <summary>
        /// Builds an ARCA request DTO for the dummy authorization flow.
        /// Used when ARCA is disabled to simulate authorization without contacting ARCA.
        /// </summary>
        private ArcaRequestDTO BuildArcaRequest(FiscalDocument doc)
        {
            // VAT must be grouped by aliquot (VATId) - ARCA requires aggregated amounts
            // not one entry per item
            var vatGrouped = doc.Items
                .GroupBy(i => i.VATId)
                .Select(g => new ArcaVAT
                {
                    id = g.Key,
                    baseAmount = g.Sum(x => x.VATBase),
                    amount = g.Sum(x => x.VATAmount)
                })
                .ToList();

            return new ArcaRequestDTO
            {
                header = new ArcaHeader
                {
                    recordCount = 1,
                    pointOfSale = doc.PointOfSale,
                    documentType = doc.InvoiceType
                },
                detail = new List<ArcaDetail>
                {
                    new ArcaDetail
                    {
                        concept = 1, // Products
                        buyerDocumentType = doc.BuyerDocumentType,
                        buyerDocumentNumber = doc.BuyerDocumentNumber,
                        invoiceFrom = doc.InvoiceFrom,
                        invoiceTo = doc.InvoiceTo,
                        invoiceDate = doc.Date.ToString("yyyyMMdd"),
                        totalAmount = doc.TotalAmount,
                        netAmount = doc.NetAmount,
                        vatAmount = doc.VATAmount,
                        nonTaxableAmount = doc.NonTaxableAmount,
                        exemptAmount = doc.ExemptAmount,
                        otherTaxesAmount = doc.OtherTaxesAmount,
                        vat = vatGrouped
                    }
                }
            };
        }

        /// <summary>
        /// Generates a random invoice number for dummy authorization.
        /// This is replaced by WSFE's next-number logic in real mode.
        /// </summary>
        private long GenerateInvoiceNumber()
        {
            // Dummy only - generates based on UTC ticks to ensure uniqueness
            return DateTime.UtcNow.Ticks % 100000000;
        }

        public async Task<FiscalDocumentDTO?> GetByIdAsync(int id)
        {
            var doc = await _unitOfWork.FiscalDocumentRepository.GetByIdAsync(id);
            return doc == null ? null : MapToDTO(doc);
        }

        public async Task<FiscalDocumentDTO?> GetBySalesOrderIdAsync(int salesOrderId)
        {
            var doc = await _unitOfWork.FiscalDocumentRepository.GetBySalesOrderIdAsync(salesOrderId);
            return doc == null ? null : MapToDTO(doc);
        }

        public async Task<IReadOnlyList<FiscalDocumentDTO>> GetCreditNotesBySaleIdAsync(int saleId)
        {
            var docs = await _unitOfWork.FiscalDocumentRepository
                .GetBySaleIdIdAsync(saleId, FiscalDocumentType.CreditNote);

            return docs.Select(MapToDTO).ToList();
        }

        public async Task<IReadOnlyList<FiscalDocumentDTO>> GetDebitNotesBySaleIdAsync(int saleId)
        {
            var docs = await _unitOfWork.FiscalDocumentRepository
                .GetBySaleIdIdAsync(saleId, FiscalDocumentType.DebitNote);

            return docs.Select(MapToDTO).ToList();
        }

        private FiscalDocumentDTO MapToDTO(FiscalDocument doc)
        {
            var dto = new FiscalDocumentDTO
            {
                Id = doc.Id,
                DocumentNumber = doc.DocumentNumber,
                InvoiceType = doc.InvoiceType,
                PointOfSale = doc.PointOfSale,
                Date = doc.Date,
                Cae = doc.CAE,
                CaeExpiration = doc.CAEExpiration,
                BuyerDocumentType = doc.BuyerDocumentType,
                BuyerDocumentNumber = doc.BuyerDocumentNumber,
                NetAmount = doc.NetAmount,
                VatAmount = doc.VATAmount,
                ExemptAmount = doc.ExemptAmount,
                NonTaxableAmount = doc.NonTaxableAmount,
                OtherTaxesAmount = doc.OtherTaxesAmount,
                TotalAmount = doc.TotalAmount,
                SalesOrderId = doc.SalesOrderId,
                Currency = doc.Currency,
                ExchangeRate = doc.ExchangeRate,
                IssuerTaxId = doc.IssuerTaxId,
                ReferencedInvoiceType = doc.ReferencedInvoiceType,
                ReferencedPointOfSale = doc.ReferencedPointOfSale,
                ReferencedInvoiceNumber = doc.ReferencedInvoiceNumber,
                Items = doc.Items.Select(i => new FiscalDocumentItemDTO
                {
                    Sku = i.Sku,
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    VatId = i.VATId,
                    VatBase = i.VATBase,
                    VatAmount = i.VATAmount,
                    DispatchCode = i.DispatchCode,
                    Warranty = i.Warranty
                }).ToList()
            };

            dto.ArcaQrUrl = GenerateAfipQrUrl(doc, dto);

            return dto;
        }

        /// <summary>
        /// Generates an AFIP QR URL for printing on fiscal documents.
        /// The QR contains encoded fiscal document information as per AFIP standard.
        /// </summary>
        private string GenerateAfipQrUrl(FiscalDocument doc, FiscalDocumentDTO dto)
        {
            var qrData = new QrData
            {
                ver = 1,                              // QR version
                fecha = doc.Date.ToString("yyyy-MM-dd"), // Document date
                cuit = long.Parse(doc.IssuerTaxId),    // Issuer CUIT
                ptoVta = doc.PointOfSale,             // Point of sale
                tipoCmp = doc.InvoiceType,            // Document type code
                nroCmp = doc.InvoiceFrom,             // Document number
                importe = doc.TotalAmount,            // Total amount
                moneda = doc.Currency,                // Currency
                ctz = doc.ExchangeRate,               // Exchange rate
                tipoDocRec = doc.BuyerDocumentType,   // Receiver document type
                nroDocRec = doc.BuyerDocumentNumber,  // Receiver document number
                tipoCodAut = "E",                     // Authorization type code (E=Electronic)
                codAut = doc.CAE                      // Authorization code (CAE)
            };

            var json = JsonSerializer.Serialize(qrData);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            return $"https://www.arca.gob.ar/fe/qr/?p={base64}";
        }

        /// <summary>
        /// Internal class for QR data structure.
        /// Follows AFIP/ARCA QR format specification.
        /// </summary>
        private class QrData
        {
            public int ver { get; set; }
            public string fecha { get; set; } = "";
            public long cuit { get; set; }
            public int ptoVta { get; set; }
            public int tipoCmp { get; set; }
            public long nroCmp { get; set; }
            public decimal importe { get; set; }
            public string moneda { get; set; } = "PES";
            public decimal ctz { get; set; } = 1;
            public int tipoDocRec { get; set; }
            public long nroDocRec { get; set; }
            public string tipoCodAut { get; set; } = "E";
            public string codAut { get; set; } = "";
        }

        /// <summary>
        /// Validates buyer document data based on invoice type requirements.
        /// Different invoice types have different document type requirements per ARCA rules.
        /// </summary>
        private void ValidateBuyerDataByInvoiceType(FiscalDocumentCreateDTO dto)
        {
            // Minimal safety rules per ARCA specifications
            // (can be refined later with official ARCA document type codes)

            // Factura A (code 1): requires CUIT as receiver
            // Rule: BuyerDocumentType must be CUIT and number must be 11 digits
            if (dto.InvoiceType == 1)
            {
                // CUIT must be 11 digits
                if (dto.BuyerDocumentNumber < 10000000000 || dto.BuyerDocumentNumber > 99999999999)
                    throw new FiscalValidationException("Factura A requires a valid 11-digit CUIT as buyer document number.");

                // BuyerDocumentType must be set (exact ARCA code validation via FEParamGetTiposDoc)
                if (dto.BuyerDocumentType <= 0)
                    throw new FiscalValidationException("Factura A requires a valid buyer document type.");
            }

            // Factura B (code 6): flexible - can be DNI, CUIT, or CF depending on rules and amounts
            // Minimal rule: document type must be valid and number > 0
            if (dto.InvoiceType == 6)
            {
                if (dto.BuyerDocumentType <= 0)
                    throw new FiscalValidationException("Factura B requires a valid buyer document type.");
            }
        }

    }


}
