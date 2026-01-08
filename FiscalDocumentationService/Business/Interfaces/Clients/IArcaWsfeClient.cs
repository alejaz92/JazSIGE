

using FiscalDocumentationService.Business.Models.Arca;

namespace FiscalDocumentationService.Business.Interfaces.Clients
{
    public interface IArcaWsfeClient
    {
        Task<WsfeDummyResult> DummyAsync(CancellationToken ct = default);
        Task<IReadOnlyList<WsfeCbteTypeItem>> GetInvoiceTypesAsync(long issuerCuit, CancellationToken ct = default);
        Task<long> GetLastAuthorizedAsync(long issuerCuit, int pointOfSale, int cbteType, CancellationToken ct = default);
        Task<CaeResponse> RequestCaeAsync(long issuerCuit, WsfeCaeRequest req, CancellationToken ct = default);

    }

    public sealed class WsfeDummyResult
    {
        public string AppServer { get; init; } = "";
        public string DbServer { get; init; } = "";
        public string AuthServer { get; init; } = "";
    }

    public sealed class WsfeCbteTypeItem
    {
        public int Id { get; init; }
        public string Description { get; init; } = "";
    }
}
