using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Infrastructure.Interfaces
{
    public interface IReceiptRepository : IGenericRepository<Receipt>
    {
        Task<Receipt?> GetWithPaymentsAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<Receipt>> GetByPartyAsync(int partyId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    }
}
