using AuthService.Infrastructure.Models;

namespace StockService.Infrastructure.Interfaces
{
    public interface IStockRepository
    {
        Task AddAsync(Stock stock);
        Task<Stock?> GetByArticleAndwarehouseAsync(int articleId, int warehouseId);
        Task UpdateAsync(Stock stock);
    }
}
