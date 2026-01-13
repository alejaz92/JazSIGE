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
            // 1) Company fiscal settings
            var companyFiscal = await _companyClient.GetCompanyFiscalSettingsAsync();
            if (companyFiscal == null)
                throw new FiscalConfigurationException("Company fiscal settings not found.");

            if (companyFiscal.ArcaEnabled && companyFiscal.ArcaPointOfSale == null)
                throw new FiscalConfigurationException("ARCA is enabled but PointOfSale is not configured in CompanyInfo.");

            // Guard: environment match (config vs CompanyService)
            var arcaEnv = (_arcaOptions.Value.Environment ?? "Homologation").Trim();
            if (!string.IsNullOrWhiteSpace(companyFiscal.ArcaEnvironment) &&
                !companyFiscal.ArcaEnvironment.Equals(arcaEnv, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"ARCA environment mismatch. Config='{arcaEnv}' CompanyService='{companyFiscal.ArcaEnvironment}'.");
            }

            // 2) Validations (your existing ones)
            if (dto.Items == null || dto.Items.Count == 0)
                throw new FiscalValidationException("At least one item is required.");

            if (dto.TotalAmount <= 0)
                throw new FiscalValidationException("TotalAmount must be greater than 0.");

            if (dto.BuyerDocumentNumber <= 0)
                throw new FiscalValidationException("BuyerDocumentNumber must be greater than 0.");

            ValidateBuyerDataByInvoiceType(dto);

            var calcTotal = dto.NetAmount + dto.VatAmount + dto.ExemptAmount + dto.NonTaxableAmount + dto.OtherTaxesAmount;
            if (Math.Abs(calcTotal - dto.TotalAmount) > 0.01m)
                throw new ArgumentException($"TotalAmount mismatch. Expected {calcTotal} but got {dto.TotalAmount}.");

            // 3) Type mapping (as you already do)
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

            // 3.1) Validate reference fields for credit/debit notes
            if (type == FiscalDocumentType.CreditNote || type == FiscalDocumentType.DebitNote)
            {
                if (!dto.ReferencedInvoiceType.HasValue)
                    throw new FiscalValidationException($"{type} requires ReferencedInvoiceType (tipo de comprobante referenciado).");
                if (!dto.ReferencedPointOfSale.HasValue)
                    throw new FiscalValidationException($"{type} requires ReferencedPointOfSale (punto de venta del comprobante referenciado).");
                if (!dto.ReferencedInvoiceNumber.HasValue || dto.ReferencedInvoiceNumber <= 0)
                    throw new FiscalValidationException($"{type} requires ReferencedInvoiceNumber (número del comprobante referenciado).");
            }

            // 4) Idempotency (only invoices)
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

            // 5) Issuer CUIT digits
            var issuerCuitDigits = Regex.Replace(companyFiscal.TaxId ?? "", @"\D", "");
            if (!long.TryParse(issuerCuitDigits, out var issuerCuit))
                throw new FiscalConfigurationException($"Invalid Company TaxId format: '{companyFiscal.TaxId}'");

            // 6) Create doc in Pending first (audit-friendly)
            var document = new FiscalDocument
            {
                PointOfSale = companyFiscal.ArcaPointOfSale ?? 1,

                InvoiceType = dto.InvoiceType,
                BuyerDocumentType = dto.BuyerDocumentType,
                BuyerDocumentNumber = dto.BuyerDocumentNumber,

                // Numbers will be set later (dummy or WSFE next)
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

                // IMPORTANT: this must exist in your DTO or be derived.
                // If your DTO field name differs, replace here:
                ReceiverVatConditionId = dto.ReceiverVatConditionId,

                // Referencia a factura original (para notas de débito y crédito)
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

            // 7) Emit (Dummy or WSFE)
            if (!companyFiscal.ArcaEnabled)
            {
                // Dummy flow: simulates ARCA authorization without contacting ARCA
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
                // --- Real WSFE flow ---
                document.ArcaLastInteractionAt = DateTime.UtcNow;

                // 7.1) Next number from WSFE
                var last = await _arcaWsfeClient.GetLastAuthorizedAsync(
                    issuerCuit,
                    document.PointOfSale,
                    document.InvoiceType);

                var next = last + 1;
                document.InvoiceFrom = next;
                document.InvoiceTo = next;
                document.DocumentNumber = $"{document.PointOfSale:0000}-{next:00000000}";

                // 7.2) Map to WsfeCaeRequest
                var wsfeReq = BuildWsfeCaeRequest(document);

                document.ArcaRequestJson = JsonSerializer.Serialize(wsfeReq);

                // 7.3) Request CAE
                var caeResp = await _arcaWsfeClient.RequestCaeAsync(issuerCuit, wsfeReq);

                document.ArcaResponseJson = JsonSerializer.Serialize(caeResp);

                // 7.4) Apply response
                document.ArcaLastInteractionAt = DateTime.UtcNow;

                if (caeResp.Errors != null && caeResp.Errors.Count > 0)
                    document.ArcaErrorsJson = JsonSerializer.Serialize(caeResp.Errors);

                if (caeResp.Events != null && caeResp.Events.Count > 0)
                    document.ArcaObservationsJson = JsonSerializer.Serialize(caeResp.Events);

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
                    // "R" (Rejected) or other
                    document.ArcaStatus = "Rejected";
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return MapToDTO(document);
        }



        private WsfeCaeRequest BuildWsfeCaeRequest(FiscalDocument doc)
        {
            // Concept: 1 = Products (tu caso)
            const int concept = 1;

            // DateOnly required by WsfeCaeRequest
            var cbteDate = DateOnly.FromDateTime(doc.Date);

            // VAT items must be grouped by aliquot (same as you did in dummy BuildArcaRequest)
            var vatItems = doc.Items
                .GroupBy(i => i.VATId)
                .Select(g => new WsfeVatItem(
                    VatId: g.Key,
                    BaseAmount: g.Sum(x => x.VATBase),
                    VatAmount: g.Sum(x => x.VATAmount)
                ))
                .ToList();

            // WSFE rule in your client: if NetAmount > 0 then VatItems must exist and sum must match VatAmount.
            // If NetAmount == 0, we send empty list to avoid unnecessary IVA node.
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
                // Campos de referencia para notas de débito y crédito
                ReferencedCbteType: doc.ReferencedInvoiceType,
                ReferencedPointOfSale: doc.ReferencedPointOfSale,
                ReferencedCbteNumber: doc.ReferencedInvoiceNumber
            );
        }

        private ArcaRequestDTO BuildArcaRequest(FiscalDocument doc)
        {
            // 2) VAT must be grouped by aliquot (VATId) - not one entry per item
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
                        concept = 1, // Products (for this company)
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

        private long GenerateInvoiceNumber()
        {
            // Dummy only. Will be replaced by WSFE next-number logic.
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

        private string GenerateAfipQrUrl(FiscalDocument doc, FiscalDocumentDTO dto)
        {
            var qrData = new QrData
            {
                ver = 1,
                fecha = doc.Date.ToString("yyyy-MM-dd"),
                cuit = long.Parse(doc.IssuerTaxId),
                ptoVta = doc.PointOfSale,
                tipoCmp = doc.InvoiceType,
                nroCmp = doc.InvoiceFrom,
                importe = doc.TotalAmount,
                moneda = doc.Currency,
                ctz = doc.ExchangeRate,
                tipoDocRec = doc.BuyerDocumentType,
                nroDocRec = doc.BuyerDocumentNumber,
                tipoCodAut = "E",
                codAut = doc.CAE
            };

            var json = JsonSerializer.Serialize(qrData);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            return $"https://www.arca.gob.ar/fe/qr/?p={base64}";
        }

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

        private void ValidateBuyerDataByInvoiceType(FiscalDocumentCreateDTO dto)
        {
            // Minimal safety rules (we can refine later with official ARCA doc type codes)

            // Invoice A (code 1): requires CUIT as receiver in most real scenarios.
            // We enforce: BuyerDocumentType must be "CUIT" and number must be 11 digits.
            if (dto.InvoiceType == 1)
            {
                // CUIT must be 11 digits
                if (dto.BuyerDocumentNumber < 10000000000 || dto.BuyerDocumentNumber > 99999999999)
                    throw new FiscalValidationException("Factura A requires a valid 11-digit CUIT as buyer document number.");

                // BuyerDocumentType must be set (we'll enforce exact ARCA code later via FEParamGetTiposDoc)
                if (dto.BuyerDocumentType <= 0)
                    throw new FiscalValidationException("Factura A requires a valid buyer document type.");
            }


            // Invoice B (code 6): can be DNI/CUIT/CF depending on rules and amounts.
            // Minimal rule: document type must be > 0 and number > 0 (you already validate number > 0)
            if (dto.InvoiceType == 6)
            {
                if (dto.BuyerDocumentType <= 0)
                    throw new FiscalValidationException("Factura B requires a valid buyer document type.");
            }
        }

    }


}
