using FiscalDocumentationService.Infrastructure.Models;

namespace FiscalDocumentationService.Infrastructure.Interfaces
{
    public interface IFiscalDocumentRepository
    {
        Task<FiscalDocument> CreateAsync(FiscalDocument document);
        Task<FiscalDocument?> GetByIdAsync(int id);
        Task<List<FiscalDocument>> GetByRelatedIdAsync(int relatedId, FiscalDocumentType? type = null);
        Task<FiscalDocument?> GetBySalesOrderIdAsync(int salesOrderId);
        Task<decimal> GetCreditNotesTotalForAsync(int relatedId);
        Task<decimal> GetDebitNotesTotalForAsync(int relatedId);
    }
}
