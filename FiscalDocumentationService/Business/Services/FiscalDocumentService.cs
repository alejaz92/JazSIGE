using FiscalDocumentationService.Business.Interfaces;
using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models;
using FiscalDocumentationService.Business.Models.Arca;
using FiscalDocumentationService.Infrastructure.Interfaces;
using FiscalDocumentationService.Infrastructure.Models;
using System.Text;
using System.Text.Json;

namespace FiscalDocumentationService.Business.Services
{
    public class FiscalDocumentService : IFiscalDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IArcaServiceClient _arcaClient;

        public FiscalDocumentService(IUnitOfWork unitOfWork, IArcaServiceClient arcaClient)
        {
            _unitOfWork = unitOfWork;
            _arcaClient = arcaClient;
        }

        public async Task<FiscalDocumentDTO> CreateAsync(FiscalDocumentCreateDTO dto)
        {
            var invoiceNumber = GenerateInvoiceNumber();

            var document = new FiscalDocument
            {
                PointOfSale = dto.PointOfSale,
                InvoiceType = dto.InvoiceType,
                BuyerDocumentType = dto.BuyerDocumentType,
                BuyerDocumentNumber = dto.BuyerDocumentNumber,
                InvoiceFrom = invoiceNumber,
                InvoiceTo = invoiceNumber,
                Date = DateTime.Now,

                NetAmount = dto.NetAmount,
                VATAmount = dto.VatAmount,
                ExemptAmount = dto.ExemptAmount,
                NonTaxableAmount = dto.NonTaxableAmount,
                OtherTaxesAmount = dto.OtherTaxesAmount,
                TotalAmount = dto.TotalAmount,
                SalesOrderId = dto.SalesOrderId,

                Currency = dto.Currency,
                ExchangeRate = dto.ExchangeRate,
                IssuerTaxId = dto.IssuerTaxId.Replace("-", ""),

                Items = dto.Items.Select(i => new FiscalDocumentItem
                {
                    Sku = i.Sku,
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    VATId = i.VatId,
                    VATBase = i.VatBase,
                    VATAmount = i.VatAmount,
                    DispatchCode = i.DispatchCode, // Optional dispatch code
                    Warranty = i.Warranty
                }).ToList()
            };

            // Armar solicitud ARCA
            var arcaRequest = BuildArcaRequest(document);

            // Llamar cliente dummy ARCA
            var arcaResponse = await _arcaClient.AuthorizeAsync(arcaRequest);

            // Aplicar CAE
            document.CAE = arcaResponse.cae;
            document.CAEExpiration = DateTime.ParseExact(arcaResponse.caeExpirationDate, "yyyyMMdd", null);
            document.DocumentNumber = $"{document.PointOfSale:0000}-{document.InvoiceFrom:00000000}";

            //var qrUrl = GenerateAfipQrUrl(document, dto);
         


            await _unitOfWork.FiscalDocumentRepository.CreateAsync(document);
            await _unitOfWork.SaveChangesAsync();

            return MapToDTO(document);
        }
        private ArcaRequestDTO BuildArcaRequest(FiscalDocument doc)
        {
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
                        concept = 1,
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
                        vat = doc.Items.Select(i => new ArcaVAT
                        {
                            id = i.VATId,
                            baseAmount = i.VATBase,
                            amount = i.VATAmount
                        }).ToList()
                    }
                }
            };
        }
        private long GenerateInvoiceNumber()
        {
            return DateTime.UtcNow.Ticks % 100_000_000;
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
        public async Task<FiscalDocumentDTO> CreateCreditNoteAsync(CreditNoteCreateDTO dto)
        {
            var baseInvoice = await _unitOfWork.FiscalDocumentRepository.GetByIdAsync(dto.RelatedFiscalDocumentId)
                ?? throw new ArgumentException("Related invoice not found");

            // Validaciones mínimas: mismo receptor
            if (baseInvoice.BuyerDocumentType != dto.BuyerDocumentType ||
                baseInvoice.BuyerDocumentNumber != dto.BuyerDocumentNumber)
                throw new InvalidOperationException("Buyer must match the original invoice.");

            // (Opcional) Validar que no se exceda el total de la factura con las NC acumuladas
            var creditAcc = await _unitOfWork.FiscalDocumentRepository.GetCreditNotesTotalForAsync(baseInvoice.Id);
            if (creditAcc + dto.TotalAmount > baseInvoice.TotalAmount)
                throw new InvalidOperationException("Credit amount exceeds original invoice total.");

            var number = GenerateInvoiceNumber();
            var document = new FiscalDocument
            {
                Type = FiscalDocumentType.CreditNote,
                RelatedFiscalDocumentId = baseInvoice.Id,

                PointOfSale = dto.PointOfSale,
                InvoiceType = MapNoteTypeFromInvoice(baseInvoice.InvoiceType, isCredit: true),
                BuyerDocumentType = dto.BuyerDocumentType,
                BuyerDocumentNumber = dto.BuyerDocumentNumber,

                InvoiceFrom = number,
                InvoiceTo = number,
                Date = DateTime.Now,

                NetAmount = dto.NetAmount,
                VATAmount = dto.VatAmount,
                ExemptAmount = dto.ExemptAmount,
                NonTaxableAmount = dto.NonTaxableAmount,
                OtherTaxesAmount = dto.OtherTaxesAmount,
                TotalAmount = dto.TotalAmount,

                Currency = dto.Currency,
                ExchangeRate = dto.ExchangeRate,
                IssuerTaxId = dto.IssuerTaxId.Replace("-", ""),

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
                }).ToList()
            };

            var arcaRequest = BuildArcaRequest(document); // Reusa tu builder
            var arcaResponse = await _arcaClient.AuthorizeAsync(arcaRequest);

            document.CAE = arcaResponse.cae;
            document.CAEExpiration = DateTime.ParseExact(arcaResponse.caeExpirationDate, "yyyyMMdd", null);
            document.DocumentNumber = $"{document.PointOfSale:0000}-{document.InvoiceFrom:00000000}";

            await _unitOfWork.FiscalDocumentRepository.CreateAsync(document);
            await _unitOfWork.SaveChangesAsync();

            return MapToDTO(document);
        }
        public async Task<FiscalDocumentDTO> CreateDebitNoteAsync(DebitNoteCreateDTO dto)
        {
            var baseInvoice = await _unitOfWork.FiscalDocumentRepository.GetByIdAsync(dto.RelatedFiscalDocumentId)
                ?? throw new ArgumentException("Related invoice not found");

            if (baseInvoice.BuyerDocumentType != dto.BuyerDocumentType ||
                baseInvoice.BuyerDocumentNumber != dto.BuyerDocumentNumber)
                throw new InvalidOperationException("Buyer must match the original invoice.");

            var debitAcc = await _unitOfWork.FiscalDocumentRepository.GetDebitNotesTotalForAsync(baseInvoice.Id);
            // (Opcional) Podés limitar el tope si tu negocio lo requiere. Por defecto, ND puede superar.
            // if (debitAcc + dto.TotalAmount > someLimit) ...

            var number = GenerateInvoiceNumber();
            var document = new FiscalDocument
            {
                Type = FiscalDocumentType.DebitNote,
                RelatedFiscalDocumentId = baseInvoice.Id,

                PointOfSale = dto.PointOfSale,
                InvoiceType = MapNoteTypeFromInvoice(baseInvoice.InvoiceType, isCredit: false),
                BuyerDocumentType = dto.BuyerDocumentType,
                BuyerDocumentNumber = dto.BuyerDocumentNumber,

                InvoiceFrom = number,
                InvoiceTo = number,
                Date = DateTime.Now,

                NetAmount = dto.NetAmount,
                VATAmount = dto.VatAmount,
                ExemptAmount = dto.ExemptAmount,
                NonTaxableAmount = dto.NonTaxableAmount,
                OtherTaxesAmount = dto.OtherTaxesAmount,
                TotalAmount = dto.TotalAmount,

                Currency = dto.Currency,
                ExchangeRate = dto.ExchangeRate,
                IssuerTaxId = dto.IssuerTaxId.Replace("-", ""),

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
                }).ToList()
            };

            var arcaRequest = BuildArcaRequest(document);
            var arcaResponse = await _arcaClient.AuthorizeAsync(arcaRequest);

            document.CAE = arcaResponse.cae;
            document.CAEExpiration = DateTime.ParseExact(arcaResponse.caeExpirationDate, "yyyyMMdd", null);
            document.DocumentNumber = $"{document.PointOfSale:0000}-{document.InvoiceFrom:00000000}";

            await _unitOfWork.FiscalDocumentRepository.CreateAsync(document);
            await _unitOfWork.SaveChangesAsync();

            return MapToDTO(document);
        }
        public async Task<IReadOnlyList<FiscalDocumentDTO>> GetCreditNotesByRelatedIdAsync(int relatedId)
        {
            var docs = await _unitOfWork.FiscalDocumentRepository
                .GetByRelatedIdAsync(relatedId, FiscalDocumentType.CreditNote);

            return docs.Select(MapToDTO).ToList();
        }
        public async Task<IReadOnlyList<FiscalDocumentDTO>> GetDebitNotesByRelatedIdAsync(int relatedId)
        {
            var docs = await _unitOfWork.FiscalDocumentRepository
                .GetByRelatedIdAsync(relatedId, FiscalDocumentType.DebitNote);

            return docs.Select(MapToDTO).ToList();
        }


        private FiscalDocumentDTO MapToDTO(FiscalDocument doc)
        {
            return new FiscalDocumentDTO
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
                ArcaQrUrl = GenerateAfipQrUrl(doc, new FiscalDocumentCreateDTO
                {
                    IssuerTaxId = doc.IssuerTaxId,
                    Currency = doc.Currency,
                    ExchangeRate = doc.ExchangeRate
                }),
                Items = doc.Items.Select(i => new FiscalDocumentItemDTO
                {
                    Sku = i.Sku,
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    VatId = i.VATId,
                    VatBase = i.VATBase,
                    VatAmount = i.VATAmount,
                    DispatchCode = i.DispatchCode, // Optional dispatch code
                    Warranty = i.Warranty
                }).ToList()
            };
        }
        private string GenerateAfipQrUrl(FiscalDocument doc, FiscalDocumentCreateDTO dto)
        {
            var qrData = new AfipQrData
            {
                fecha = doc.Date.ToString("yyyyMMdd"),
                cuit = dto.IssuerTaxId,
                ptoVta = doc.PointOfSale,
                tipoCmp = doc.InvoiceType,
                nroCmp = doc.InvoiceFrom,
                importe = doc.TotalAmount,
                moneda = dto.Currency,
                ctz = dto.ExchangeRate,
                tipoDocRec = doc.BuyerDocumentType,
                nroDocRec = doc.BuyerDocumentNumber,
                codAut = doc.CAE
            };

            string json = JsonSerializer.Serialize(qrData);
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            return $"https://www.afip.gob.ar/fe/qr/?p={base64}";
        }
        private class AfipQrData
        {
            public int ver { get; set; } = 1;
            public string fecha { get; set; } = ""; // yyyyMMdd
            public string cuit { get; set; }
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

        // Mapea el tipo de NOTA (AFIP) a partir del tipo de FACTURA base.
        // A: 01 -> ND 02, NC 03
        // B: 06 -> ND 07, NC 08
        // C: 11 -> ND 12, NC 13
        // M: 51 -> ND 52, NC 53
        private int MapNoteTypeFromInvoice(int baseInvoiceType, bool isCredit)
        {
            return (baseInvoiceType, isCredit) switch
            {
                (01, false) => 02,
                (01, true) => 03,
                (06, false) => 07,
                (06, true) => 08,
                (11, false) => 12,
                (11, true) => 13,
                (51, false) => 52,
                (51, true) => 53,
                _ => throw new InvalidOperationException($"Unsupported base invoice type for notes: {baseInvoiceType}")
            };
        }

    }
}
