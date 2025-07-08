using Microsoft.EntityFrameworkCore;
using StockService.Infrastructure.Data;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Repositories
{
    public class StockByDispatchRepository : IStockByDispatchRepository
    {
        private readonly StockDbContext _context;

        public StockByDispatchRepository(StockDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StockByDispatch entry)
        {
            await _context.StockByDispatches.AddAsync(entry);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(StockByDispatch entry)
        {
            _context.StockByDispatches.Update(entry);
            await _context.SaveChangesAsync();
        }
        public async Task<StockByDispatch?> GetByArticleAndDispatchAsync(int articleId, int? dispatchId) => await _context.StockByDispatches
                .FirstOrDefaultAsync(s => s.ArticleId == articleId && s.DispatchId == dispatchId);
        public async Task<List<StockByDispatch>> GetAvailableByArticleOrderedAsync(int articleId) => await _context.StockByDispatches
                .Where(s => s.ArticleId == articleId && s.Quantity > 0)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();
        public async Task<StockByDispatch?> GetLatestByArticleAsync(int articleId) => await _context.StockByDispatches
                .Where(s => s.ArticleId == articleId)
                .OrderByDescending(s => s.DispatchId)
                .FirstOrDefaultAsync();

    }
}
