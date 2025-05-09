using CatalogService.Business.Services;

namespace CatalogService.Business.Interfaces
{
    public interface IStockServiceClient
    {
        Task<bool> HasStockAsync(int articleId);
    }
}

