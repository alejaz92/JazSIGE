using AccountingService.Infrastructure.Models;
using JazSIGE.Accounting.Infrastructure.Interfaces;

namespace AccountingService.Infrastructure.Interfaces
{
    public interface IAllocationBatchRepository : IGenericRepository<AllocationBatch>
    {
        Task<AllocationBatch?> GetFullAsync(int id); // incluye Items
    }
}
