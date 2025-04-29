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

        [HttpGet("line-group/{lineGroupId}")]
        public async Task<IActionResult> GetByLineGroupIdAsync(int lineGroupId)
        {
            var lines = await _lineService.GetByLineGroupIdAsync(lineGroupId);
            if (lines == null || !lines.Any())
            {
                return NotFound("No lines found for the given line group ID.");
            }
            return Ok(lines);
        }
    }
}
