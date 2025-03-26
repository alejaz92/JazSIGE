using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Line;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineController : BaseController<Line, LineDTO, LineCreateDTO>
    {
        private readonly ILineService _lineService;

        public LineController(ILineService lineService) : base(lineService)
        {
            _lineService = lineService;
        }
    }
}
