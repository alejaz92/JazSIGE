using AccountingService.Business.Models.Common;
using AccountingService.Business.Models.Ledger;

namespace AccountingService.Business.Interfaces
{
    public interface ICustomerAccountQueryService
    {
        Task<PagedResult<LedgerItemDTO>> GetLedgerAsync(
            int customerId,
            DateTime? from,
            DateTime? to,
            Infrastructure.Models.Ledger.LedgerDocumentKind? kind,
            Infrastructure.Models.Ledger.LedgerDocumentStatus? status,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task<PagedResult<PendingItemDTO>> GetPendingAsync(
            int customerId,
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task<BalancesDTO> GetBalancesAsync(
            int customerId,
            CancellationToken ct = default);
    }
}
