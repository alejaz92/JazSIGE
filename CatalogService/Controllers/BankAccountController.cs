using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.BankAccount;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountController : BaseController<BankAccount, BankAccountDTO, BankAccountCreateDTO>
    {
        private readonly IBankAccountService _bankAccountService;
        public BankAccountController(IBankAccountService bankAccountService) : base(bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }
    }
}
