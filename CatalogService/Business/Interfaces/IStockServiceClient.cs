using CatalogService.Business.Services;

namespace CatalogService.Business.Interfaces
{
    public interface IStockServiceClient
    {
        Task<bool> HasStockByArticleAsync(int articleId);
        Task<bool> HasStockByWarehouseAsync(int warehouseId);
    }
}

