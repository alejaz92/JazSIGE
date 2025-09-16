using AccountingService.Infrastructure.Data;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories
{
    public class ReceiptRepository : GenericRepository<Receipt>, IReceiptRepository
    {
        private readonly AccountingDbContext _ctx;

        public ReceiptRepository(AccountingDbContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public Task<Receipt?> GetWithPaymentsAsync(int id, CancellationToken ct = default)
            => _ctx.Receipts.AsNoTracking()
                            .Include(r => r.PaymentLines)
                            .FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<IEnumerable<Receipt>> GetByPartyAsync(int partyId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            var q = _ctx.Receipts.AsNoTracking()
                                 .Where(r => r.PartyId == partyId);

            if (from.HasValue) q = q.Where(r => r.Date >= from.Value);
            if (to.HasValue) q = q.Where(r => r.Date <= to.Value);

            return await q.OrderByDescending(r => r.Date).ToListAsync(ct);
        }
    }
}
