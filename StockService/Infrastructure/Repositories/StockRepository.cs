using AuthService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using StockService.Infrastructure.Data;
using StockService.Infrastructure.Interfaces;

namespace StockService.Infrastructure.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly StockDbContext _context;
        public StockRepository(StockDbContext context)
        {
            _context = context;
        }

        public async Task<Stock?> GetByArticleAndwarehouseAsync(int articleId, int warehouseId) => await _context.Stocks
                .FirstOrDefaultAsync(s => s.ArticleId == articleId && s.WarehouseId == warehouseId);

        public async Task AddAsync(Stock stock)
        {
            await _context.Stocks.AddAsync(stock);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Stock stock)
        {
            _context.Stocks.Update(stock);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Stock>> GetAllByArticleAsync(int articleId) => await _context.Stocks
                .Where(s => s.ArticleId == articleId)
                .ToListAsync();
    }
}
