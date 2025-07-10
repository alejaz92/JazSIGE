using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Interfaces
{
    public interface IStockTransferRepository
    {
        Task AddAsync(StockTransfer stockTransfer);
        Task<string> GenerateNextCodeAsync();
        Task<IEnumerable<StockTransfer>> GetAllAsync();
        Task<StockTransfer?> GetByIdAsync(int id);
    }
}
