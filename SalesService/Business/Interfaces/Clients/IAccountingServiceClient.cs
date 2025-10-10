using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IAccountingServiceClient
    {
        Task UpsertExternalAsync(AccountingExternalUpsertDTO dto, CancellationToken ct = default);

        // imputar con recibos previos
        Task<IReadOnlyList<ReceiptCreditDTO>> GetReceiptCreditsAsync(int partyId, CancellationToken ct = default);
        Task CoverInvoiceWithReceiptsAsync(CoverInvoiceRequest dto, CancellationToken ct = default);
    }
}
