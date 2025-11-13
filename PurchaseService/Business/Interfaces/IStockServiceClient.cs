
using PurchaseService.Business.Models;

namespace PurchaseService.Business.Interfaces
{
    public interface IStockServiceClient
    {
        Task<StockApplyAdjustmentResultDTO?> ApplyPendingAdjustmentsAsync(int purchaseId, int userId, IEnumerable<PendingStockAdjustmentItemDTO> items);
        Task RegisterPendingStockAsync(PendingStockEntryCreateDTO dto);
        Task RegisterPendingStockConsolidatedAsync(RegisterPendingStockInputDTO dto, int userId);
        Task RegisterPurchaseMovementsAsync(int userId, int warehouseId, List<(int articleId, decimal quantity)> items, string reference, int? dispatchId);
    }
}
