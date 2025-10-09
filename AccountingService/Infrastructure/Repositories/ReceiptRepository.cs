using AccountingService.Infrastructure.Data;
using AccountingService.Infrastructure.Models;
using JazSIGE.Accounting.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories
{
    public class ReceiptRepository : GenericRepository<Receipt>, IReceiptRepository
    {
        public ReceiptRepository(AccountingDbContext ctx) : base(ctx) { }

        public async Task<Receipt?> GetFullAsync(int id)
        {
            return await _ctx.Receipts
                .Include(r => r.Payments)
                .Include(r => r.Allocations)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
