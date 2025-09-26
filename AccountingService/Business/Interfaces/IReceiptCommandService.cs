using AccountingService.Business.Models.Ledger;

namespace AccountingService.Business.Interfaces
{
    public interface IReceiptCommandService
    {
        Task<ReceiptDTO> CreateReceiptAsync(ReceiptCreateDTO request, CancellationToken ct = default);
        Task<ReceiptDTO?> VoidReceiptAsync(int receiptId, CancellationToken ct = default);

        // <--- firma correcta: usa el DTO con ReceiptId explícito
        Task AllocateAsync(ReceiptAllocationCreateDTO request, CancellationToken ct = default);

        Task DeallocateAsync(int allocationId, CancellationToken ct = default);
    }
}
