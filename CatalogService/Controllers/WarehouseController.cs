using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Warehouse;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : BaseController<Warehouse, WarehouseDTO, WarehouseCreateDTO>
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService) : base(warehouseService) 
        {
            _warehouseService = warehouseService;
        }
    }
}
