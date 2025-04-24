using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.PostalCode;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostalCodeController : BaseController<PostalCode, PostalCodeDTO, PostalCodeCreateDTO>
    {
        private readonly IPostalCodeService _postalCodeService;

        public PostalCodeController(IPostalCodeService postalCodeService) : base(postalCodeService)
        {
            _postalCodeService = postalCodeService;
        }

        // get PostalCode by CityId
        [HttpGet("city/{cityId}")]
        public async Task<IActionResult> GetByCityIdAsync(int cityId)
        {
            var postalCodes = await _postalCodeService.GetByCityIdAsync(cityId);
            if (postalCodes == null || !postalCodes.Any())
            {
                return NotFound("No postal codes found for the given city ID.");
            }
            return Ok(postalCodes);
        }

    }
}
