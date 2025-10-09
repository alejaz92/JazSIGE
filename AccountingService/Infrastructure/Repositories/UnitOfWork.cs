using AccountingService.Infrastructure.Data;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Models;
using JazSIGE.Accounting.Infrastructure.Interfaces;
using JazSIGE.Accounting.Infrastructure.Models;
using JazSIGE.Accounting.Infrastructure.Repositories;

namespace AccountingService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AccountingDbContext _ctx;

        public UnitOfWork(
            AccountingDbContext ctx,
            ILedgerDocumentRepository ledgerDocuments,
            IReceiptRepository receipts,
            IAllocationBatchRepository allocationBatches)
        {
            _ctx = ctx;
            LedgerDocuments = ledgerDocuments;
            Receipts = receipts;
            AllocationBatches = allocationBatches;

            ReceiptPayments = new GenericRepository<ReceiptPayment>(_ctx);
            ReceiptAllocations = new GenericRepository<ReceiptAllocation>(_ctx);
            AllocationItems = new GenericRepository<AllocationItem>(_ctx);
        }

        public ILedgerDocumentRepository LedgerDocuments { get; }
        public IReceiptRepository Receipts { get; }
        public IAllocationBatchRepository AllocationBatches { get; }

        // 🔹 Nuevos repositorios
        public IGenericRepository<ReceiptPayment> ReceiptPayments { get; }
        public IGenericRepository<ReceiptAllocation> ReceiptAllocations { get; }
        public IGenericRepository<AllocationItem> AllocationItems { get; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _ctx.SaveChangesAsync(ct);
    }
}
