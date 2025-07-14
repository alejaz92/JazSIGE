using FiscalDocumentationService.Infrastructure.Data;
using FiscalDocumentationService.Infrastructure.Interfaces;
using FiscalDocumentationService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace FiscalDocumentationService.Infrastructure.Repositories
{
    public class FiscalDocumentRepository : GenericRepository<FiscalDocument>, IFiscalDocumentRepository
    {
        private readonly FiscalDocumentationDbContext _context;

        public FiscalDocumentRepository(FiscalDocumentationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<FiscalDocument?> GetBySaleIdAsync(int saleId)
        {
            return await _context.FiscalDocuments
                .Include(d => d.Articles)
                .FirstOrDefaultAsync(d => d.SaleId == saleId);
        }

        public async Task<string?> GetLastDocumentNumberAsync(string documentType, string pointOfSale, string documentLetter)
        {
            return await _context.FiscalDocuments
                .Where(d => d.DocumentType == documentType &&
                            d.PointOfSale == pointOfSale &&
                            d.DocumentLetter == documentLetter)
                .OrderByDescending(d => d.DocumentNumber)
                .Select(d => d.DocumentNumber)
                .FirstOrDefaultAsync();
        }
    }
}
