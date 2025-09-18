using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using AccountingService.Infrastructure.Data;
using AccountingService.Infrastructure.Interfaces;
using AccountingService.Infrastructure.Repositories;

namespace AccountingService.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AccountingDbContext _ctx;
        private ILedgerDocumentRepository? _ledgerDocuments;
        private IReceiptRepository? _receipts;
        private IAllocationRepository? _allocations;
        private IDbContextTransaction? _trx;

        public UnitOfWork(AccountingDbContext ctx)
        {
            _ctx = ctx;
        }

        // Exposición de repos específicos (lazy)
        public ILedgerDocumentRepository LedgerDocuments =>
            _ledgerDocuments ??= new LedgerDocumentRepository(_ctx);

        public IReceiptRepository Receipts =>
            _receipts ??= new ReceiptRepository(_ctx);

        public IAllocationRepository Allocations =>
            _allocations ??= new AllocationRepository(_ctx);

        // Persistencia
        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            _ctx.SaveChangesAsync(ct);

        // Transacciones
        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_trx != null) return _trx;
            _trx = await _ctx.Database.BeginTransactionAsync(ct);
            return _trx;
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (_trx == null) return;
            await _trx.CommitAsync(ct);
            await _trx.DisposeAsync();
            _trx = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (_trx == null) return;
            await _trx.RollbackAsync(ct);
            await _trx.DisposeAsync();
            _trx = null;
        }
    }
}
