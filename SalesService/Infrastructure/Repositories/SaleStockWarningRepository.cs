using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SalesService.Infrastructure.Data;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;

namespace SalesService.Infrastructure.Repositories
{
    public class SaleStockWarningRepository : GenericRepository<SaleStockWarning>, ISaleStockWarningRepository
    {
        private readonly SalesDbContext _context;

        public SaleStockWarningRepository(SalesDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SaleStockWarning>> GetActiveBySaleIdAsync(int saleId)
        {
            // Returns all non-resolved warnings for a given sale.
            return await _context.SaleStockWarnings
                .Where(w => w.SaleId == saleId && !w.IsResolved)
                .ToListAsync();
        }

        public async Task<SaleStockWarning?> GetActiveBySaleAndArticleAsync(int saleId, int articleId)
        {
            // Returns the active warning for the given sale and article, if any.
            return await _context.SaleStockWarnings
                .FirstOrDefaultAsync(w =>
                    w.SaleId == saleId &&
                    w.ArticleId == articleId &&
                    !w.IsResolved);
        }
    }
}
