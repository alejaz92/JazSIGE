using AccountingService.Business.Models.Ledger;
using AccountingService.Business.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptService _service;

        public ReceiptsController(IReceiptService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReceiptResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ReceiptRequestCreateDTO request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, result);
        }
    }
}
