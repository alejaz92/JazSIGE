using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Receipts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AllocationsController : ControllerBase
    {
        private readonly IAllocationService _allocationsService;
        private readonly ILogger<AllocationsController> _logger;

        public AllocationsController(IAllocationService allocationsService, ILogger<AllocationsController> logger)
        {
            _allocationsService = allocationsService;
            _logger = logger;
        }

        [HttpPost("cover-invoice")]
        public async Task<IActionResult> CoverInvoice([FromBody] CoverInvoiceDTO body, CancellationToken ct)
        {
            try
            {
                if (body is null)
                    return BadRequest("Invalid payload.");

                if (body.Items is null || body.Items.Count == 0)
                    return BadRequest("At least one source receipt must be provided.");

                await _allocationsService.CoverInvoiceWithReceiptsAsync(body);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error covering invoice");
                return StatusCode(500, new { message = "Unexpected error covering invoice." });
            }
        }
    }
}
