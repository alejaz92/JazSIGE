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
        public ProvinceController(IProvinceService service) : base(service)
        {
        }
    }
}
