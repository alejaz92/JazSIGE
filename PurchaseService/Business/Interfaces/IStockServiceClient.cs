
using PurchaseService.Business.Models;

namespace PurchaseService.Business.Interfaces
{
    public interface IStockServiceClient
    {
        Task RegisterPendingStockAsync(PendingStockEntryCreateDTO dto);
        Task RegisterPendingStockAsync(PendingStockEntryCreateDTO dto);
        Task RegisterPurchaseMovementsAsync(int userId, int warehouseId, List<(int articleId, decimal quantity)> items, string reference, int? dispatchId);
    }
}
