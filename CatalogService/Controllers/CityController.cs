using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.City;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : BaseController<City, CityDTO, CityCreateDTO>
    {
        private readonly ICityService _cityService;
        public CityController(ICityService cityService) : base(cityService)
        {
            _cityService = cityService;
        }
    }
}
