using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Supplier;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : BaseController<Supplier, SupplierDTO, SupplierCreateDTO>
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService) : base(supplierService) 
        {
            _supplierService = supplierService;
        }
    }
}
