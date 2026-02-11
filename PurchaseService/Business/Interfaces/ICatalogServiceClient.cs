
using PurchaseService.Business.Models.Clients;

namespace PurchaseService.Business.Interfaces
{
    public interface ICatalogServiceClient
    {
        Task<List<ArticleListDTO>> GetAllArticlesAsync();
        Task<List<SupplierListDTO>> GetAllSuppliersAsync();
        Task<List<WarehouseListDTO>> GetAllWarehousesAsync();
        Task<string?> GetArticleNameAsync(int articleId);
        Task<string?> GetSupplierNameAsync(int supplierId);
        Task<string?> GetWarehouseNameAsync(int warehouseId);
    }
}
