using AccountingService.Business.Models.Ledger;

namespace AccountingService.Business.Interfaces
{
    public interface ICustomerPendingQueryService
    {
        Task<List<PendingDocumentDTO>> GetPendingAsync(int customerId, CancellationToken ct = default);
    }
}
