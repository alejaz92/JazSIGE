using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Country;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : BaseController<Country, CountryDTO, CountryCreateDTO>
    {
        private readonly ICountryService _countryService;
        public CountryController(ICountryService countryService) : base(countryService)
        {
            _countryService = countryService;
        }
    }
    {
    }
}
