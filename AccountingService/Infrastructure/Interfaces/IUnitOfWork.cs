using AccountingService.Infrastructure.Models;
using JazSIGE.Accounting.Infrastructure.Interfaces;

namespace AccountingService.Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        ILedgerDocumentRepository LedgerDocuments { get; }
        IReceiptRepository Receipts { get; }
        IAllocationBatchRepository AllocationBatches { get; }

        IGenericRepository<ReceiptPayment> ReceiptPayments { get; }
        IGenericRepository<ReceiptAllocation> ReceiptAllocations { get; }
        IGenericRepository<AllocationItem> AllocationItems { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
