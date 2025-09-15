using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Bank;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankController : BaseController<Bank, BankDTO, BankCreateDTO>
    {
        private readonly IBankService _bankService;
        public BankController(IBankService bankService) : base(bankService)
        {
            _bankService = bankService;
        }
    }
}
