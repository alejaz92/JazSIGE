using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Infrastructure.Interfaces
{
    public interface ILedgerDocumentRepository : IGenericRepository<LedgerDocument>
    {
        Task<bool> ExistsBySourceAsync(SourceKind sourceKind, long sourceId, CancellationToken ct = default);
        Task<IEnumerable<LedgerDocument>> GetByPartyAsync(int partyId, CancellationToken ct = default);
        Task<LedgerDocument?> GetByReceiptIdAsync(int receiptId, CancellationToken ct = default);
        Task<LedgerDocument?> GetBySourceAsync(SourceKind sourceKind, long sourceId, CancellationToken ct = default);
        IQueryable<LedgerDocument> Query();
    }
}
