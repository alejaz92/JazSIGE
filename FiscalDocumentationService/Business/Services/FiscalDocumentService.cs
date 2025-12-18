using FiscalDocumentationService.Business.Interfaces;
using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models;
using FiscalDocumentationService.Business.Models.Arca;
using FiscalDocumentationService.Infrastructure.Interfaces;
using FiscalDocumentationService.Infrastructure.Models;
using System.Text;
using System.Text.Json;
using static FiscalDocumentationService.Business.Exceptions.FiscalDocumentationException;

namespace FiscalDocumentationService.Business.Services
{
    public class FiscalDocumentService : IFiscalDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IArcaServiceClient _arcaClient;
        private readonly ICompanyServiceClient _companyClient;

        public FiscalDocumentService(IUnitOfWork unitOfWork, IArcaServiceClient arcaClient, ICompanyServiceClient companyClient)
        {
            _unitOfWork = unitOfWork;
            _arcaClient = arcaClient;
            _companyClient = companyClient;
        }

        public async Task<FiscalDocumentDTO> CreateAsync(FiscalDocumentCreateDTO dto)
        {
            // get fiscal settings from company service
            var companyFiscal = await _companyClient.GetCompanyFiscalSettingsAsync();
            if (companyFiscal == null)
                throw new FiscalConfigurationException("Company fiscal settings not found.");

            // Safety gate: if ARCA is enabled, POS must be configured
            if (companyFiscal.ArcaEnabled && companyFiscal.ArcaPointOfSale == null)
                throw new FiscalConfigurationException("ARCA is enabled but PointOfSale is not configured in CompanyInfo.");

            var useDummy = !companyFiscal.ArcaEnabled;

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




            // Define type (invoice, credit note, debit note) based on dto.InvoiceType
            var type = dto.InvoiceType switch
            {
                1 => FiscalDocumentType.Invoice,       // A
                6 => FiscalDocumentType.Invoice,       // B
                11 => FiscalDocumentType.Invoice,      // C
                51 => FiscalDocumentType.Invoice,      // M
                2 => FiscalDocumentType.DebitNote,     // A
                7 => FiscalDocumentType.DebitNote,     // B
                12 => FiscalDocumentType.DebitNote,    // C
                52 => FiscalDocumentType.DebitNote,    // M
                3 => FiscalDocumentType.CreditNote,    // A
                8 => FiscalDocumentType.CreditNote,    // B
                13 => FiscalDocumentType.CreditNote,   // C
                53 => FiscalDocumentType.CreditNote,   // M
                _ => throw new ArgumentException("Invalid Invoice Type")
            };

            // 1) Idempotency (only for INVOICES)
            // A SalesOrder should not generate multiple invoices by accident.
            // Credit/Debit notes are allowed to be multiple, so we don't block those here.
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

            // NOTE: For now we keep the dummy invoice number generation.
            // Later, when we integrate WSFE, this will be replaced by:
            // GetLastAuthorized(PointOfSale, InvoiceType) + 1
            var invoiceNumber = GenerateInvoiceNumber();

            var document = new FiscalDocument
            {
                PointOfSale = companyFiscal.ArcaPointOfSale ?? 1,
           
                InvoiceType = dto.InvoiceType,
                BuyerDocumentType = dto.BuyerDocumentType,
                BuyerDocumentNumber = dto.BuyerDocumentNumber,
                InvoiceFrom = invoiceNumber,
                InvoiceTo = invoiceNumber,
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
                IssuerTaxId = companyFiscal.TaxId.Replace("-", ""),

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

                EmissionProvider = "Dummy",
                ArcaEnvironment = companyFiscal.ArcaEnvironment
            };

            // Build ARCA request (still dummy client, but structure matters)
            var arcaRequest = BuildArcaRequest(document);

            ArcaResponseDTO arcaResponse;

            if (useDummy)
            {
                arcaResponse = await _arcaClient.AuthorizeAsync(arcaRequest);
            }
            else
            {
                // TEMP: hasta que implementemos el real, igual llamamos al dummy
                arcaResponse = await _arcaClient.AuthorizeAsync(arcaRequest);
            }


            // Apply CAE
            document.CAE = arcaResponse.cae;
            document.CAEExpiration = DateTime.ParseExact(arcaResponse.caeExpirationDate, "yyyyMMdd", null);
            document.DocumentNumber = $"{document.PointOfSale:0000}-{document.InvoiceFrom:00000000}";

            await _unitOfWork.FiscalDocumentRepository.CreateAsync(document);
            await _unitOfWork.SaveChangesAsync();

            return MapToDTO(document);
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
