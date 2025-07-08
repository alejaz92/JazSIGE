using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Interfaces
{
    public interface IStockByDispatchRepository
    {
        Task AddAsync(StockByDispatch entry);
        Task<List<StockByDispatch>> GetAvailableByArticleOrderedAsync(int articleId);
        Task<StockByDispatch?> GetByArticleAndDispatchAsync(int articleId, int? dispatchId);
        Task<StockByDispatch?> GetLatestByArticleAsync(int articleId);
        Task UpdateAsync(StockByDispatch entry);
    }
}
