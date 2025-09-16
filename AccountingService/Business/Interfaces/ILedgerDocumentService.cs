using AccountingService.Business.Models.Ledger;

namespace AccountingService.Business.Services.Interfaces;

public interface ILedgerDocumentService
{
    Task<DocumentResponseDTO> CreateAsync(DocumentRequestCreateDTO request, CancellationToken ct = default);
}
