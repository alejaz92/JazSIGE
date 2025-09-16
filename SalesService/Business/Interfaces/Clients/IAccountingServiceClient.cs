using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IAccountingServiceClient
    {
        Task CreateLedgerDocumentAsync(AccountingDocumentCreateDTO dto, CancellationToken ct = default);
    }
}
