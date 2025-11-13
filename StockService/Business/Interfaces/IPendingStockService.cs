
using StockService.Business.Models;
using StockService.Business.Models.CommitedStock;
using StockService.Business.Models.PendingStock;

namespace StockService.Business.Interfaces
{
    public interface IPendingStockService
    {
        Task AddAsync(PendingStockEntryCreateDTO dto);
        Task<StockApplyAdjustmentResultDTO> ApplyPurchasePendingAdjustmentsAsync(PurchasePendingAdjustmentDTO dto);
        Task<List<PendingStockEntryDTO>> GetByPurchaseIdAsync(int purchaseId);
        Task<decimal> GetPendingStockByArticleAsync(int articleId);
        Task RegisterPendingStockAsync(RegisterPendingStockInputDTO dto, int userId);
    }
}
