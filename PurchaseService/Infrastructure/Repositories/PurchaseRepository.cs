

using Microsoft.EntityFrameworkCore;
using PurchaseService.Infrastructure.Data;
using PurchaseService.Infrastructure.Interfaces;
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Infrastructure.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly PurchaseDbContext _context;

        public PurchaseRepository(PurchaseDbContext context)
        {
            _context = context;
        }

        public async Task<Purchase> AddAsync(Purchase purchase)
        {
            await _context.Purchases.AddAsync(purchase);
            return purchase;
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<IEnumerable<Purchase>> GetAllAsync() => await _context.Purchases
                .Include(p => p.Articles)
                .Include(p => p.Dispatch)
                .OrderByDescending(p => p.Date)
                .ToListAsync();


        public async Task<IEnumerable<Purchase>> GetAllAsync(int pageNumber, int pageSize) => await _context.Purchases
                .Include(p => p.Articles)
                .Include(p => p.Dispatch)
                .OrderByDescending(p => p.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<int> GetTotalCountAsync() => await _context.Purchases.CountAsync();

        public async Task<Purchase?> GetByIdAsync(int id) => await _context.Purchases
                .Include(p => p.Articles)
                .Include(p => p.Dispatch)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Purchase>> GetPendingStockAsync() => await _context.Purchases
                .Include(p => p.Articles)
                .Include(p => p.Dispatch)
                .Where(p => !p.StockUpdated)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
    }
}
   
