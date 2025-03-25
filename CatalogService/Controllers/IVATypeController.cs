using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.IVAType;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IVATypeController : BaseController<IVAType, IVATypeDTO, IVATypeCreateDTO>
    {
        private readonly IIVATypeService _ivaTypeService;
        public IVATypeController(IIVATypeService ivaTypeService) : base(ivaTypeService)
        {
            _ivaTypeService = ivaTypeService;
        }
    }
}
