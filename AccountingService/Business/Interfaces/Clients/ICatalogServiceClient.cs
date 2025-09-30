using AccountingService.Business.Models.Clients;

namespace AccountingService.Business.Interfaces.Clients
{
    public interface ICatalogServiceClient
    {
        Task<CustomerDTO?> GetCustomerByIdAsync(int customerId);
    }
}
