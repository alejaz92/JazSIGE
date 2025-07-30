using FiscalDocumentationService.Infrastructure.Data;
using FiscalDocumentationService.Infrastructure.Interfaces;
using FiscalDocumentationService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace FiscalDocumentationService.Infrastructure.Repositories
{
    public class FiscalDocumentRepository : IFiscalDocumentRepository
    {
        private readonly FiscalDocumentationDbContext _context;
        public FiscalDocumentRepository(FiscalDocumentationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<FiscalDocument> CreateAsync(FiscalDocument document)
        {
            _context.FiscalDocuments.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }
        public async Task<FiscalDocument?> GetByIdAsync(int id) => await _context.FiscalDocuments
                .Include(d => d.Items)
                .FirstOrDefaultAsync(d => d.Id == id);
        public async Task<FiscalDocument?> GetBySalesOrderIdAsync(int salesOrderId) => await _context.FiscalDocuments
                .Include(d => d.Items)
                .FirstOrDefaultAsync(d => d.SalesOrderId == salesOrderId);
    }
}
