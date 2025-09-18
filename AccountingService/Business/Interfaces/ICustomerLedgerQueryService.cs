using AccountingService.Business.Models.Common;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Interfaces
{
    public interface ICustomerLedgerQueryService
    {
        Task<PagedResult<CustomerLedgerItemDTO>> GetCustomerLedgerAsync(
        int customerId,
        DateTime? from,
        DateTime? to,
        LedgerDocumentKind? kind,
        LedgerDocumentStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
    }
}
