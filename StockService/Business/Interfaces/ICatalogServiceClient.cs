
using StockService.Business.Models.Clients;

namespace StockService.Business.Interfaces
{
    public interface ICatalogServiceClient
    {
        Task<bool> ArticleExistsAsync(int articleId);
        Task<string?> GetArticleNameAsync(int articleId);
        Task<TransportDTO> GetTransportAsync(int transportId);
        Task<string?> GetTransportNameAsync(int transportId);
        Task<string?> GetWarehouseNameAsync(int warehouseId);
        Task<bool> WarehouseExistsAsync(int warehouseId);
    }
}
