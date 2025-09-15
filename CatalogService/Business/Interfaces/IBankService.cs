using CatalogService.Business.Models.Bank;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IBankService : IGenericService<Bank, BankDTO, BankCreateDTO>
    {
    }
}
