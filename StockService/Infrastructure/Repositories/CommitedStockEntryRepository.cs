using Microsoft.EntityFrameworkCore;
using StockService.Infrastructure.Data;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Repositories
{
    public class CommitedStockEntryRepository : ICommitedStockEntryRepository
    {
        private readonly StockDbContext _context;
        public CommitedStockEntryRepository(StockDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(CommitedStockEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            await _context.CommitedStockEntries.AddAsync(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CommitedStockEntry>> GetBySaleIdAsync(int saleId) => await _context.CommitedStockEntries
                .Where(c => c.SaleId == saleId && (c.Quantity - c.Delivered) != 0)
                .ToListAsync();

        public async Task MarkCompletedDeliveryAsync(int id, decimal quantity)
        {
            var entry = await _context.CommitedStockEntries.FirstOrDefaultAsync(c => c.Id == id);
            if (entry != null)
            {
                // check if the quantity to be marked as processed is valid
                if (quantity <= 0 || quantity > entry.Remaining)
                {
                    throw new ArgumentException("Invalid quantity to mark as processed.");
                }
                entry.Delivered += quantity;
                entry.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalCommitedStockByArticleIdAsync(int articleId) => await _context.CommitedStockEntries
                .Where(c => c.ArticleId == articleId && (c.Quantity - c.Delivered) != 0)
                .SumAsync(c => (c.Quantity - c.Delivered));
    }
}
