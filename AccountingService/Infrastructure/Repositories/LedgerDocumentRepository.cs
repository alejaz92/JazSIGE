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

        public Task<LedgerDocument?> GetByFiscalIdAsync(int fiscalDocumentId, CancellationToken ct = default)
            => _ctx.LedgerDocuments.AsNoTracking()
                                   .FirstOrDefaultAsync(d => d.FiscalDocumentId == fiscalDocumentId, ct);

        public Task<bool> ExistsByFiscalIdAsync(int fiscalDocumentId, CancellationToken ct = default)
            => _ctx.LedgerDocuments.AnyAsync(d => d.FiscalDocumentId == fiscalDocumentId, ct);

        public async Task<IEnumerable<LedgerDocument>> GetByPartyAsync(int partyId, CancellationToken ct = default)
            => await _ctx.LedgerDocuments.AsNoTracking()
                                         .Where(d => d.PartyId == partyId)
                                         .OrderByDescending(d => d.DocumentDate)
                                         .ToListAsync(ct);
    }
}
