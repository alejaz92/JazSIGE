using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Infrastructure.Interfaces
{
    public interface IAllocationRepository : IGenericRepository<Allocation>
    {
        Task<Dictionary<int, decimal>> GetAppliedByDocumentsAsync(IEnumerable<int> documentIds, CancellationToken ct = default);
        Task<Dictionary<int, decimal>> GetAppliedByReceiptsAsync(IEnumerable<int> receiptIds, CancellationToken ct = default);
        IQueryable<Allocation> Query();
    }
}
