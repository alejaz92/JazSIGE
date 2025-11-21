using StockService.Business.Models;
using StockService.Business.Services;
using System.Threading.Tasks;

namespace StockService.Business.Interfaces
{
    public interface IStockService
    {
        Task<decimal> GetStockAsync(int articleId, int warehouseId);
        Task<decimal> GetStockSummaryAsync(int articleId);
        
        Task<PaginatedResultDTO<StockMovementDTO>> GetMovementsByArticleAsync(int articleId, int page, int pageSize);
        Task<IEnumerable<StockDTO>> GetStockByArticleAsync(int articleId);
        //Task<decimal> GetStockSummaryByWarehouseAsync(int warehouseId);
        Task<List<DispatchStockDetailDTO>> RegisterMovementAsync(StockMovementCreateDTO dto, int userId);
        Task<IEnumerable<StockByWarehouseDTO>> GetStockByWarehouseAsync(int warehouseId);
        Task<IEnumerable<ArticleCostsDTO>> GetArticleCostsAsync(int articleId);
        //Task<decimal> GetAvailableStockByArticleAsync(int articleId);
        //Task<decimal> GetAvailableStockByArticleAndWarehouseAsync(int articleId, int warehouseId);
    }
}
