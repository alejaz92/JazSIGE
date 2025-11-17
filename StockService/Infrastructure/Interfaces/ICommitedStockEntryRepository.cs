using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Interfaces
{
    public interface ICommitedStockEntryRepository
    {
        Task AddAsync(CommitedStockEntry entry);
        Task<CommitedStockEntry?> GetBySaleIdAndArticleIdAsync(int saleId, int articleId);
        Task<List<CommitedStockEntry>> GetBySaleIdAsync(int saleId);
        Task<List<CommitedStockEntry>> GetRemainingByArticleAsync(int articleId);
        Task<decimal> GetTotalCommitedStockByArticleIdAsync(int articleId);
        Task<List<(int SalesOrderId, decimal Remaining)>> ListRemainingByArticleAsync(int articleId);
        Task MarkCompletedDeliveryAsync(int id, decimal quantity);
        Task<decimal> SumRemainingByArticleAsync(int articleId);
        Task UpdateQuantityAsync(int id, decimal newQuantity);
    }
}
