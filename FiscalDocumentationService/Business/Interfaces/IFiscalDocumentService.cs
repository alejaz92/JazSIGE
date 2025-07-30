using FiscalDocumentationService.Business.Models;

namespace FiscalDocumentationService.Business.Interfaces
{
    public interface IFiscalDocumentService
    {
        Task<FiscalDocumentDTO> CreateAsync(FiscalDocumentCreateDTO dto);
        Task<FiscalDocumentDTO?> GetByIdAsync(int id);
        Task<FiscalDocumentDTO?> GetBySalesOrderIdAsync(int salesOrderId);
    }
}
