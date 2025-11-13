using SalesService.Infrastructure.Models.Sale;

namespace SalesService.Infrastructure.Interfaces
{
    public interface ISaleStockWarningRepository : IGenericRepository<SaleStockWarning>
    {
        Task<SaleStockWarning?> GetActiveBySaleAndArticleAsync(int saleId, int articleId);
        Task<IEnumerable<SaleStockWarning>> GetActiveBySaleIdAsync(int saleId);
    }
}
