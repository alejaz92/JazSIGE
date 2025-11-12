using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Interfaces
{
    public interface IPendingStockEntryRepository
    {
        Task AddAsync(PendingStockEntry entry);
        Task<List<PendingStockEntry>> GetByPurchaseIdAsync(int purchaseId);
        Task<decimal> GetTotalPendingStockByArticleIdAsync(int articleId);
        Task<List<PendingStockEntry>> GetUnprocessedByPurchaseArticleAsync(int purchaseId, int articleId);
        Task MarkAsProcessedAsync(int id);
        Task<decimal> SumUnprocessedByArticleAsync(int articleId);
    }
}
