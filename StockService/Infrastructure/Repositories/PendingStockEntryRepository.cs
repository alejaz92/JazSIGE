using Microsoft.EntityFrameworkCore;
using StockService.Infrastructure.Data;
using StockService.Infrastructure.Interfaces;
using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Repositories
{
    public class PendingStockEntryRepository : IPendingStockEntryRepository
    {
        private readonly StockDbContext _context;
        public PendingStockEntryRepository(StockDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(PendingStockEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            await _context.PendingStockEntries.AddAsync(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PendingStockEntry>> GetByPurchaseIdAsync(int purchaseId) => await _context.PendingStockEntries
                .Where(p => p.PurchaseId == purchaseId && !p.IsProcessed)
                .ToListAsync();

        public async Task MarkAsProcessedAsync(int id)
        {
            var entry = await _context.PendingStockEntries.FirstOrDefaultAsync(p => p.Id == id);
            if (entry != null)
            {
                entry.IsProcessed = true;
                entry.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalPendingStockByArticleIdAsync(int articleId) => await _context.PendingStockEntries
                .Where(p => p.ArticleId == articleId && !p.IsProcessed)
                .SumAsync(p => p.Quantity);

        // Sum of non-processed pending for a given article (global)
        public async Task<decimal> SumUnprocessedByArticleAsync(int articleId)
        {
            return await _context.PendingStockEntries
                .Where(p => p.ArticleId == articleId && !p.IsProcessed)
                .SumAsync(p => p.Quantity);
        }

        // Returns non-processed pending entries for a purchase/article ordered FIFO
        public async Task<List<PendingStockEntry>> GetUnprocessedByPurchaseArticleAsync(int purchaseId, int articleId)
        {
            return await _context.PendingStockEntries
                .Where(p => p.PurchaseId == purchaseId && p.ArticleId == articleId && !p.IsProcessed)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        // Allows the service to remove tracked entries with zero quantity.
        public void Remove(PendingStockEntry entry) =>
            // NOTE: entry is expected to be tracked by this DbContext
            _context.PendingStockEntries.Remove(entry);

        // Allows batching multiple in-memory changes before persisting.
        public Task SaveChangesAsync() => _context.SaveChangesAsync();

    }
}
