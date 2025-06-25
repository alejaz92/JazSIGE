
using StockService.Business.Models;

namespace StockService.Business.Interfaces
{
    public interface IPendingStockService
    {
        Task AddAsync(PendingStockEntryCreateDTO dto);
        Task<List<PendingStockEntryDTO>> GetByPurchaseIdAsync(int purchaseId);
        Task<decimal> GetPendingStockByArticleAsync(int articleId);
        Task RegisterPendingStockAsync(RegisterPendingStockInputDTO dto, int userId);
    }
}
