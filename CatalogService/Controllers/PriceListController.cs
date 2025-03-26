using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.PriceList;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceListController : BaseController<PriceList, PriceListDTO, PriceListCreateDTO>
    {
        private readonly IPriceListService _priceListService;

        public PriceListController(IPriceListService priceListService) : base(priceListService)
        {
            _priceListService = priceListService;
        }
    }
}
