using AccountingService.Business.Models.Ledger;

namespace AccountingService.Business.Interfaces
{
    public interface IDocumentIntakeService
    {
        Task<DocumentDTO> IngestFiscalAsync(FiscalDocumentCreateDTO request, CancellationToken ct = default);
        Task<DocumentDTO?> VoidFiscalAsync(Infrastructure.Models.Ledger.SourceKind sourceKind, long sourceDocumentId, CancellationToken ct = default);
    }
}
