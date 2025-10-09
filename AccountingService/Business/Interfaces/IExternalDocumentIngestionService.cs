using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Business.Interfaces
{
    public interface IExternalDocumentIngestionService
    {
        // Idempotente: crea/actualiza el espejo del doc fiscal en LedgerDocuments
        Task<int> UpsertFiscalDocumentAsync(PartyType partyType, int partyId,
            LedgerDocumentKind kind, int externalRefId, string externalRefNumber, DateTime documentDate,
            decimal amountARS, string currency = "ARS", decimal fxRate = 1m);
    }
}
