
namespace StockService.Business.Interfaces
{
    public interface IStockAvailabilityService
    {
        Task<decimal> GetAvailableStockByArticleAndWarehouseAsync(int articleId, int warehouseId);
        Task<decimal> GetAvailableStockByArticleAsync(int articleId);
    }
}
