
namespace PurchaseService.Business.Interfaces
{
    public interface IStockServiceClient
    {
        Task RegisterPurchaseMovementsAsync(int userId, int warehouseId, List<(int articleId, decimal quantity)> items, string reference, int? dispatchId);
    }
}
