using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.GrossIncomeType;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrossIncomeTypeController : BaseController<GrossIncomeType, GrossIncomeTypeDTO, GrossIncomeTypeCreateDTO>
    {
        private readonly IGrossIncomeTypeService _grossIncomeTypeService;
        public GrossIncomeTypeController(IGrossIncomeTypeService grossIncomeTypeService) : base(grossIncomeTypeService)
        {
            _grossIncomeTypeService = grossIncomeTypeService;
        }
    }
}
