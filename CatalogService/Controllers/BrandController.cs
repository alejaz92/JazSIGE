using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Brand;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : BaseController<Brand, BrandDTO, BrandCreateDTO>
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService) : base(brandService)
        {
            _brandService = brandService;
        }
    }
}
