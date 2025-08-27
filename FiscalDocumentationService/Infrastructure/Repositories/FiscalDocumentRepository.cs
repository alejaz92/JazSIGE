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

        public async Task<decimal> GetCreditNotesTotalForAsync(int relatedId) => await _context.FiscalDocuments
                .Where(d => d.RelatedFiscalDocumentId == relatedId && d.Type == FiscalDocumentType.CreditNote)
                .SumAsync(d => d.TotalAmount);

        public async Task<decimal> GetDebitNotesTotalForAsync(int relatedId) => await _context.FiscalDocuments
                .Where(d => d.RelatedFiscalDocumentId == relatedId && d.Type == FiscalDocumentType.DebitNote)
                .SumAsync(d => d.TotalAmount);
    }
}
