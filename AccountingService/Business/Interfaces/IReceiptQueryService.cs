using AccountingService.Business.Models.Ledger;

namespace AccountingService.Business.Interfaces
{
    public interface IReceiptQueryService
    {
        Task<ReceiptDetailDTO> GetDetailAsync(int receiptId, CancellationToken ct = default);
    }
}
