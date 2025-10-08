using static AccountingService.Business.Models.Ledger.CreditItemsAndApplyDTO;

namespace AccountingService.Business.Interfaces
{
    public interface ICreditsQueryService
    {
        Task<IReadOnlyList<CreditItemDTO>> GetAvailableAsync(
            int customerId,
            decimal? minAmountBase = null,
            CancellationToken ct = default);
    }
}
