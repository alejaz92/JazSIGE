using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.LineGroup;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineGroupController : BaseController<LineGroup, LineGroupDTO, LineGroupCreateDTO>
    {
        private readonly ILineGroupService _lineGroupService;
        public LineGroupController(ILineGroupService lineGroupService) : base(lineGroupService)
        {
        }
    }
}
