using AccountingService.Infrastructure.Data;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories
{
    public class AllocationRepository : GenericRepository<Allocation>, IAllocationRepository
    {
        private readonly AccountingDbContext _context;
        public AllocationRepository(AccountingDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Dictionary<int, decimal>> GetAppliedByDocumentsAsync(IEnumerable<int> documentIds, CancellationToken ct = default)
        {
           return await _context.Allocations
                .Where(a => documentIds.Contains(a.DebitDocumentId))
                .GroupBy(a => a.DebitDocumentId)
                .Select(g => new { g.Key, Sum = g.Sum(x => x.AmountBase) })
                .ToDictionaryAsync(x => x.Key, x => x.Sum, ct);
        }
    }
}
