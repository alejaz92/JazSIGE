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
    }
}
