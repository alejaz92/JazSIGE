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

    }
}
