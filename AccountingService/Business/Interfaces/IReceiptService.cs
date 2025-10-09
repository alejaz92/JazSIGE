using AccountingService.Business.Models.Receipts;

namespace AccountingService.Business.Interfaces
{
    public interface IReceiptService
    {
        Task<ReceiptDetailDTO> CreateAsync(ReceiptCreateDTO dto, string? userName = null);
        Task<ReceiptDetailDTO?> GetAsync(int receiptId);
        Task<ReceiptExportDTO?> GetExportDataAsync(int receiptId);
    }
}
