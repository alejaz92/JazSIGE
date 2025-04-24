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

        // get City by ProvinceId
        [HttpGet("province/{provinceId}")]
        public async Task<IActionResult> GetByProvinceIdAsync(int provinceId)
        {
            var cities = await _cityService.GetByProvinceIdAsync(provinceId);
            if (cities == null || !cities.Any())
            {
                return NotFound("No cities found for the given province ID.");
            }
            return Ok(cities);
        }
    }
}
