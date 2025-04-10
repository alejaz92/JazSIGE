
namespace PurchaseService.Business.Interfaces
{
    public interface ICatalogServiceClient
    {
        Task<string?> GetArticleNameAsync(int articleId);
        Task<string?> GetSupplierNameAsync(int supplierId);
        Task<string?> GetWarehouseNameAsync(int warehouseId);
    }
}
