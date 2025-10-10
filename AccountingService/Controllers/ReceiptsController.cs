using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Receipts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptService _receiptService;
        private readonly ILogger<ReceiptsController> _logger;

        public ReceiptsController(IReceiptService receiptService, ILogger<ReceiptsController> logger)
        {
            _receiptService = receiptService;
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReceiptDetailDTO>> Get(int id, CancellationToken ct)
        {
            try
            {
                var dto = await _receiptService.GetAsync(id);
                return dto is null ? NotFound() : Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving receipt {id}", id);
                return StatusCode(500, new { message = "Error retrieving receipt." });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ReceiptDetailDTO>> Create([FromBody] ReceiptCreateDTO body, CancellationToken ct)
        {
            try
            {
                if (body is null)
                    return BadRequest("Invalid payload.");

                if (body.Payments == null || body.Payments.Count == 0)
                    return BadRequest("Receipt must contain at least one payment.");

                var created = await _receiptService.CreateAsync(body);
                return CreatedAtAction(nameof(Get), new { id = created.ReceiptId }, created);
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
                _logger.LogError(ex, "Error creating receipt");
                return StatusCode(500, new { message = "Unexpected error creating receipt." });
            }
        }

        [HttpGet("{id:int}/export")]
        public async Task<ActionResult<ReceiptExportDTO>> Export(int id, CancellationToken ct)
        {
            try
            {
                var dto = await _receiptService.GetExportDataAsync(id);
                return dto is null ? NotFound() : Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting receipt {id}", id);
                return StatusCode(500, new { message = "Error exporting receipt." });
            }
        }

    }
}
