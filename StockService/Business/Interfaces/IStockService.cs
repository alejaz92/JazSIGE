using StockService.Business.Models;
using StockService.Business.Services;
using System.Threading.Tasks;

namespace StockService.Business.Interfaces
{
    public interface IStockService
    {
        Task<decimal> GetStockAsync(int articleId, int warehouseId);
        Task<decimal> GetStockSummaryAsync(int articleId);
        Task RegisterMovementAsync(StockMovementCreateDTO dto, int userId);
        Task<IEnumerable<StockMovementDTO>> GetMovementsByArticleAsync(int articleId, int page, int pageSize);
    }
}
