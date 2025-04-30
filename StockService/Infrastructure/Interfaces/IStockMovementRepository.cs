using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Interfaces
{
    public interface IStockMovementRepository
    {
        Task AddAsync(StockMovement stockMovement);
        Task<IEnumerable<StockMovement>> GetAllAsync();
        Task<IEnumerable<StockMovement>> GetByArticleAsync(int articleId);
        Task<(IEnumerable<StockMovement> Items, int TotalCount)> GetPagedWithTotalAsync(int articleId, int page, int pageSize);

    }
}
