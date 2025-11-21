using AuthService.Infrastructure.Models;

namespace StockService.Infrastructure.Interfaces
{
    public interface IStockRepository
    {
        Task AddAsync(Stock stock);
        Task<IEnumerable<Stock>> GetAllByArticleAsync(int articleId);
        Task<IEnumerable<Stock>> GetAllByWarehouseAsync(int warehouseId);
        Task<Stock?> GetByArticleAndwarehouseAsync(int articleId, int warehouseId);
        Task<decimal> GetCurrentStockByArticleAsync(int articleId);
        Task UpdateAsync(Stock stock);
    }
}
