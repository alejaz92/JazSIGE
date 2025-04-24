using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Province;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProvinceController : BaseController<Province, ProvinceDTO, ProvinceCreateDTO>
    {

        private readonly IProvinceService _service;
        public ProvinceController(IProvinceService service) : base(service)
        {
            _service = service;
        }

        // get Province by CountryId
        [HttpGet("country/{countryId}")]
        public async Task<IActionResult> GetByCountryIdAsync(int countryId)
        {
            var provinces = await _service.GetByCountryIdAsync(countryId);
            if (provinces == null || !provinces.Any())
            {
                return NotFound("No provinces found for the given country ID.");
            }
            return Ok(provinces);
        }

    }
}
