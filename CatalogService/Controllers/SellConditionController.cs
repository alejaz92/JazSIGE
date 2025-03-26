using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.SellCondition;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellConditionController : BaseController<SellCondition, SellConditionDTO, SellConditionCreateDTO>
    {
        private readonly ISellConditionService _sellConditionService;

        public SellConditionController(ISellConditionService sellConditionService) : base(sellConditionService)
        {
            _sellConditionService = sellConditionService;
        }
    }
}
