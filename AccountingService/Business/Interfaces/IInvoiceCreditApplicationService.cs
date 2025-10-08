using static AccountingService.Business.Models.Ledger.CreditItemsAndApplyDTO;

namespace AccountingService.Business.Interfaces
{
    public interface IInvoiceCreditApplicationService
    {
        Task<ApplyCreditsResult> ApplyAsync(ApplyCreditsRequest req, CancellationToken ct = default);
    }
}
