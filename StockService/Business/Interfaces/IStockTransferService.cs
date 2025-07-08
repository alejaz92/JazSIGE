using StockService.Business.Models.StockTransfer;

namespace StockService.Business.Interfaces
{
    public interface IStockTransferService
    {
        Task<int> CreateAsync(StockTransferCreateDTO dto, int userId);
        Task<IEnumerable<StockTransferDTO>> GetAllAsync();
        Task<StockTransferDTO?> GetByIdAsync(int id);
    }
}
