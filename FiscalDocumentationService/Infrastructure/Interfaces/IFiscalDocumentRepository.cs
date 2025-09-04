using FiscalDocumentationService.Infrastructure.Models;

namespace FiscalDocumentationService.Infrastructure.Interfaces
{
    public interface IFiscalDocumentRepository
    {
        Task<FiscalDocument> CreateAsync(FiscalDocument document);
        Task<FiscalDocument?> GetByIdAsync(int id);
        Task<List<FiscalDocument>> GetBySaleIdIdAsync(int saleId, FiscalDocumentType? type = null);
        Task<FiscalDocument?> GetBySalesOrderIdAsync(int salesOrderId);
        Task<decimal> GetCreditNotesTotalForAsync(int saleId);
        Task<decimal> GetDebitNotesTotalForAsync(int saleId);
    }
}
