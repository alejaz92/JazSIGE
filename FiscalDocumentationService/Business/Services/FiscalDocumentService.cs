using FiscalDocumentationService.Business.Interfaces;
using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models;
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
            // Map string type to enum
            Enum.TryParse<FiscalDocumentType>(dto.Type, ignoreCase: true, out var docType);

            var document = new FiscalDocument
            {
                Type = docType,
                Date = DateTime.Now,
                CustomerName = dto.CustomerName,
                CustomerCUIT = dto.CustomerCUIT,
                CustomerIVAType = dto.CustomerIVAType,
                NetAmount = dto.NetAmount,
                VATAmount = dto.VATAmount,
                TotalAmount = dto.TotalAmount,
                SalesOrderId = dto.SalesOrderId,
                Items = dto.Items.Select(i => new FiscalDocumentItem
                {
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    VAT = i.VAT
                }).ToList()
            };

            // Obtener CAE simulado desde ARCA
            var (cae, caeExpiration) = await _arcaClient.AuthorizeAsync(document);
            document.CAE = cae;
            document.CAEExpiration = caeExpiration;

            // Simular numeración: ejemplo "A0001-00000023"
            document.DocumentNumber = $"A0001-{DateTime.Now.Ticks % 1_000_000:00000000}";

            await _unitOfWork.FiscalDocumentRepository.CreateAsync(document);
            await _unitOfWork.SaveChangesAsync();

            return MapToDTO(document);
        }

        public async Task<FiscalDocumentDTO?> GetByIdAsync(int id)
        {
            var document = await _unitOfWork.FiscalDocumentRepository.GetByIdAsync(id);
            return document == null ? null : MapToDTO(document);
        }

        public async Task<FiscalDocumentDTO?> GetBySalesOrderIdAsync(int salesOrderId)
        {
            var document = await _unitOfWork.FiscalDocumentRepository.GetBySalesOrderIdAsync(salesOrderId);
            return document == null ? null : MapToDTO(document);
        }

        private FiscalDocumentDTO MapToDTO(FiscalDocument doc)
        {
            return new FiscalDocumentDTO
            {
                Id = doc.Id,
                DocumentNumber = doc.DocumentNumber,
                Type = doc.Type.ToString(),
                Date = doc.Date,
                CAE = doc.CAE,
                CAEExpiration = doc.CAEExpiration,
                CustomerName = doc.CustomerName,
                CustomerCUIT = doc.CustomerCUIT,
                CustomerIVAType = doc.CustomerIVAType,
                NetAmount = doc.NetAmount,
                VATAmount = doc.VATAmount,
                TotalAmount = doc.TotalAmount,
                SalesOrderId = doc.SalesOrderId,
                Items = doc.Items.Select(i => new FiscalDocumentItemDTO
                {
                    Description = i.Description,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    VAT = i.VAT
                }).ToList()
            };
        }
    }
}
