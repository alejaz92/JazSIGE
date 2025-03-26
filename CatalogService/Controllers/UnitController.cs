using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Unit;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController : BaseController<Unit, UnitDTO, UnitCreateDTO>
    {
        private readonly IUnitService _unitService;

        public UnitController(IUnitService unitService) : base(unitService)
        {
            _unitService = unitService;
        }
    }
}
