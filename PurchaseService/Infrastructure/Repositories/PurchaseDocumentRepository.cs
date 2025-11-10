using Microsoft.EntityFrameworkCore;
using PurchaseService.Infrastructure.Data;
using PurchaseService.Infrastructure.Interfaces;
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Infrastructure.Repositories
{
    public class PurchaseDocumentRepository : IPurchaseDocumentRepository   
    {
        private readonly PurchaseDbContext _context;
        public PurchaseDocumentRepository(PurchaseDbContext context)
        {
            _context = context;
        }
        
        public async Task<PurchaseDocument?> GetByIdAsync(int id) => await _context.PurchaseDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

        public async Task<IReadOnlyList<PurchaseDocument>> GetByPurchaseIdAsync(int purchaseId, bool onlyActive = false)
        {
            var query = _context.PurchaseDocuments
                .AsNoTracking()
                .Where(d => d.PurchaseId == purchaseId);

            if (onlyActive)
                query = query.Where(d => !d.IsCanceled);

            return await query
                .OrderBy(d => d.Date)
                .ThenBy(d => d.Type)
                .ThenBy(d => d.Number)
                .ToListAsync();
        }

        public async Task<PurchaseDocument?> GetInvoiceByPurchaseIdAsync(int purchaseId, bool onlyActive = false)
        {
            var query = _context.PurchaseDocuments
                .AsNoTracking()
                .Where(d => d.PurchaseId == purchaseId && d.Type == PurchaseDocumentType.Invoice);

            if (onlyActive)
                query = query.Where(d => !d.IsCanceled);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsInvoiceAsync(int purchaseId, bool onlyActive = false)
        {
            var query = _context.PurchaseDocuments
                .Where(d => d.PurchaseId == purchaseId && d.Type == PurchaseDocumentType.Invoice);

            if (onlyActive)
                query = query.Where(d => !d.IsCanceled);

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsByNumberAsync(int purchaseId, PurchaseDocumentType type, string number, bool onlyActive = false)
        {
            var query = _context.PurchaseDocuments
                .Where(d => d.PurchaseId == purchaseId && d.Type == type && d.Number == number);

            if (onlyActive)
                query = query.Where(d => !d.IsCanceled);

            return await query.AnyAsync();
        }

        public async Task AddAsync(PurchaseDocument entity)
        {
            await _context.PurchaseDocuments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PurchaseDocument entity)
        {
            _context.PurchaseDocuments.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PurchaseDocument entity)
        {
            _context.PurchaseDocuments.Remove(entity);
            await _context.SaveChangesAsync();
        }

    }
}
