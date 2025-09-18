using AccountingService.Business.Models.Ledger;

namespace AccountingService.Business.Interfaces
{
    public interface ICustomerBalancesQueryService
    {
        Task<CustomerBalancesDTO> GetBalancesAsync(int customerId, CancellationToken ct = default);
    }
}
