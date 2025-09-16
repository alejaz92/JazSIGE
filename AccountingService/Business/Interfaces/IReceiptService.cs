
using AccountingService.Business.Models.Ledger;

namespace AccountingService.Business.Services.Interfaces;

public interface IReceiptService
{
    Task<ReceiptResponseDTO> CreateAsync(ReceiptRequestCreateDTO request, CancellationToken ct = default);
}
