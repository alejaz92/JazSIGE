
namespace StockService.Business.Interfaces
{
    public interface ICatalogValidatorService
    {
        Task<bool> ArticleExistsAsync(int articleId);
        Task<string?> GetArticleNameAsync(int articleId);
        Task<string?> GetWarehouseNameAsync(int warehouseId);
        Task<bool> WarehouseExistsAsync(int warehouseId);
    }
}
