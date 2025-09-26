using AccountingService.Infrastructure.Data;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories
{
    public class LedgerDocumentRepository : GenericRepository<LedgerDocument>, ILedgerDocumentRepository
    {
        private readonly AccountingDbContext _ctx;

        public LedgerDocumentRepository(AccountingDbContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }


        public Task<LedgerDocument?> GetBySourceAsync(SourceKind sourceKind, long sourceId, CancellationToken ct = default)
            => _ctx.LedgerDocuments.AsNoTracking()
                                   .FirstOrDefaultAsync(d => d.SourceKind == sourceKind && d.SourceDocumentId == sourceId, ct);

        public Task<bool> ExistsBySourceAsync(SourceKind sourceKind, long sourceId, CancellationToken ct = default)
            => _ctx.LedgerDocuments.AnyAsync(d => d.SourceKind == sourceKind && d.SourceDocumentId == sourceId, ct);

        // Receipts vinculados localmente
        public Task<LedgerDocument?> GetByReceiptIdAsync(int receiptId, CancellationToken ct = default)
            => _ctx.LedgerDocuments.AsNoTracking()
                                   .FirstOrDefaultAsync(d => d.ReceiptId == receiptId, ct);

        public async Task<IEnumerable<LedgerDocument>> GetByPartyAsync(int partyId, CancellationToken ct = default)
            => await _ctx.LedgerDocuments.AsNoTracking()
                                         .Where(d => d.PartyId == partyId)
                                         .OrderByDescending(d => d.DocumentDate)
                                         .ToListAsync(ct);

        public IQueryable<LedgerDocument> Query() => _ctx.LedgerDocuments.AsNoTracking();
    }
}
