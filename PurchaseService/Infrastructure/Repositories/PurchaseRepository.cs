

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
                .ThenByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        public async Task<int> GetTotalCountAsync() => await _context.Purchases.CountAsync();
        public async Task<Purchase?> GetByIdAsync(int id) => await _context.Purchases
                .Include(p => p.Articles)
                .Include(p => p.Dispatch)
                .Include(p => p.Documents)
                .FirstOrDefaultAsync(p => p.Id == id);
        public async Task<IEnumerable<Purchase_Article>> GetByArticleIdAsync(int articleId) => await _context.PurchaseArticles
                .Include(pa => pa.Purchase)
                .Where(pa => pa.ArticleId == articleId)
                .OrderByDescending(pa => pa.Purchase.Date)
                .ToListAsync();
        public async Task<IEnumerable<Purchase>> GetPendingStockAsync() => await _context.Purchases
                .Include(p => p.Articles)
                .Include(p => p.Dispatch)
                .Where(p => !p.IsDelivered)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
        public async Task MarkAsDeliveredAsync(int purchaseId)
        {
            var purchase = await _context.Purchases.FirstOrDefaultAsync(p => p.Id == purchaseId);
            if (purchase != null)
            {
                purchase.IsDelivered = true;
                await _context.SaveChangesAsync();
            }
        }              

    }
}
   
