using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IAccountingServiceClient
    {
        Task IngestFiscalAsync(AccountingFiscalIngestDTO dto, CancellationToken ct = default);
    }
}
