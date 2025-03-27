using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Customer;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : BaseController<Customer, CustomerDTO, CustomerCreateDTO>
    {
        private  readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService) : base(customerService)
        {
            _customerService = customerService;
        }
    }
}
