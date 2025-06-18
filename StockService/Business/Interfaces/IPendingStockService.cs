using StockService.Business.Models;

namespace StockService.Business.Interfaces
{
    public interface IPendingStockService
    {
        Task AddAsync(PendingStockEntryCreateDTO dto);
        Task<List<PendingStockEntryDTO>> GetByPurchaseIdAsync(int purchaseId);
        Task RegisterPendingStockAsync(RegisterPendingStockInputDTO dto, int userId);
    }
}
