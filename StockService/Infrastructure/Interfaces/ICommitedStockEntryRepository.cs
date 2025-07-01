using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Interfaces
{
    public interface ICommitedStockEntryRepository
    {
        Task AddAsync(CommitedStockEntry entry);
        Task<List<CommitedStockEntry>> GetBySaleIdAsync(int saleId);
        Task<List<CommitedStockEntry>> GetRemainingByArticleAsync(int articleId);
        Task<decimal> GetTotalCommitedStockByArticleIdAsync(int articleId);
        Task MarkCompletedDeliveryAsync(int id, decimal quantity);
    }
}
