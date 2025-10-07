using AccountingService.Business.Models.Ledger;

namespace AccountingService.Business.Interfaces
{
    public interface IManualAllocationService
    {
        Task<ManualAllocationDTO.ManualAllocationPreviewDTO> ExecuteAsync(ManualAllocationDTO.ManualAllocationExecuteDTO req, CancellationToken ct = default);
        Task<ManualAllocationDTO.ManualAllocationPreviewDTO> PreviewAsync(ManualAllocationDTO.ManualAllocationExecuteDTO req, CancellationToken ct = default);
    }
}
