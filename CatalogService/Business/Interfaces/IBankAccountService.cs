using CatalogService.Business.Models.BankAccount;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface IBankAccountService : IGenericService<BankAccount, BankAccountDTO, BankAccountCreateDTO>
    {
    }
}
