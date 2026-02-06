using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Business.Interfaces
{
    public interface IExternalDocumentIngestionService
    {
        Task CancelFiscalDocumentAsync(PartyType partyType, int externalRefId, LedgerDocumentKind kind);

        // Idempotente: crea/actualiza el espejo del doc fiscal en LedgerDocuments
        Task<int> UpsertFiscalDocumentAsync(
            PartyType partyType,
            int partyId,
            LedgerDocumentKind kind,
            int externalRefId,
            string externalRefNumber,
            DateTime documentDate,
            decimal amountARS,
            bool? isCash,
            string currency = "ARS",
            decimal fxRate = 1m
        );
    }
}
