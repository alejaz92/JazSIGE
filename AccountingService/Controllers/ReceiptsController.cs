using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptCommandService _service;
        private readonly IReceiptQueryService _queryService;

        public ReceiptsController(
            IReceiptCommandService service,
            IReceiptQueryService queryService)
        {
            _service = service;
            _queryService = queryService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReceiptDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ReceiptCreateDTO request, CancellationToken ct = default)
        {
            var result = await _service.CreateReceiptAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPost("{receiptId:int}/void")]
        [ProducesResponseType(typeof(ReceiptDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Void(int receiptId, CancellationToken ct = default)
        {
            var result = await _service.VoidReceiptAsync(receiptId, ct);
            if (result is null) return NotFound();
            return Ok(result);
        }

        // Imputación “suelta” de un recibo a un débito (factura/ND)
        [HttpPost("{receiptId:int}/allocations")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Allocate(int receiptId, [FromBody] AllocationBody body, CancellationToken ct = default)
        {
            var dto = new ReceiptAllocationCreateDTO
            {
                ReceiptId = receiptId,
                DebitDocumentId = body.DebitDocumentId,
                AmountBase = body.AmountBase
            };

            await _service.AllocateAsync(dto, ct);
            return NoContent();
        }

        // Desimputación por Id
        [HttpDelete("allocations/{allocationId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deallocate(int allocationId, CancellationToken ct = default)
        {
            // El servicio ignora si no existe (idempotente). Si preferís 404, chequeá antes.
            await _service.DeallocateAsync(allocationId, ct);
            return NoContent();
        }

        [HttpGet("{id:int}/detail")]
        [ProducesResponseType(typeof(ReceiptDetailDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetail(int id, CancellationToken ct = default)
        {
            try
            {
                var dto = await _queryService.GetDetailAsync(id, ct);
                return Ok(dto);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }
        }

        public class AllocationBody
        {
            public int DebitDocumentId { get; set; }
            public decimal AmountBase { get; set; }
        }
    }
}
