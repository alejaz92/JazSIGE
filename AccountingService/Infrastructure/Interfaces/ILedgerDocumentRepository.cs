using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Infrastructure.Interfaces
{
    public interface ILedgerDocumentRepository : IGenericRepository<LedgerDocument> 
    {
        Task<LedgerDocument?> GetByFiscalIdAsync(int fiscalDocumentId, CancellationToken ct = default);
        Task<bool> ExistsByFiscalIdAsync(int fiscalDocumentId, CancellationToken ct = default);
        Task<IEnumerable<LedgerDocument>> GetByPartyAsync(int partyId, CancellationToken ct = default);
    }
}
