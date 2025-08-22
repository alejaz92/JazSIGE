using FiscalDocumentationService.Business.Interfaces;
using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models;
using FiscalDocumentationService.Business.Models.Arca;
using FiscalDocumentationService.Infrastructure.Interfaces;
using FiscalDocumentationService.Infrastructure.Models;

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
    }
}
