using PurchaseService.Business.Models.Clients;

namespace PurchaseService.Business.Interfaces
{
    public interface IAccountingServiceClient
    {
        Task CoverInvoiceWithReceiptsAsync(CoverInvoiceRequest dto, CancellationToken ct = default);
        Task<IReadOnlyList<ReceiptCreditDTO>> GetReceiptCreditsAsync(int partyId, CancellationToken ct = default);
        Task UpsertExternalAsync(AccountingExternalUpsertDTO dto, CancellationToken ct = default);
        Task VoidExternalAsync(string ledgerDocumentKind, int externalRefId, string partyType, CancellationToken ct = default);
    }
}
