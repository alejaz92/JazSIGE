using CatalogService.Business.Models.Customer;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface ICustomerService : IGenericService<Customer, CustomerDTO, CustomerCreateDTO>
    {
    }
}
