using AccountingService.Infrastructure.Data;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories
{
    public class AllocationBatchRepository : GenericRepository<AllocationBatch>, IAllocationBatchRepository
    {
        public AllocationBatchRepository(AccountingDbContext ctx) : base(ctx) { }

        public async Task<AllocationBatch?> GetFullAsync(int id)
        {
            return await _ctx.AllocationBatches
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}
