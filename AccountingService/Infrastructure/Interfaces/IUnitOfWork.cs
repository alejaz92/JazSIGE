using Microsoft.EntityFrameworkCore.Storage;

namespace AccountingService.Infrastructure.Interfaces
{
    public interface IUnitOfWork
    {
        // Repositorios específicos
        ILedgerDocumentRepository LedgerDocuments { get; }
        IReceiptRepository Receipts { get; }
        IAllocationRepository Allocations { get; }

        // Persistencia y transacciones
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync(CancellationToken ct = default);
    }
}
