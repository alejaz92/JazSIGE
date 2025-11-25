using PurchaseService.Business.Models.Clients;

namespace PurchaseService.Business.Interfaces
{
    public interface IAccountingServiceClient
    {
        Task UpsertExternalAsync(AccountingExternalUpsertDTO dto, CancellationToken ct = default);
        Task VoidExternalAsync(string ledgerDocumentKind, int externalRefId, string partyType, CancellationToken ct = default);
    }
}
