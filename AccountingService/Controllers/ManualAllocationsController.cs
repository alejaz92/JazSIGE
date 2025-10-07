using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AccountingService.Business.Models.Ledger.ManualAllocationDTO;

namespace AccountingService.Controllers
{
    [ApiController]
    [Route("api/customers/{customerId:int}/allocations/manual")]
    public sealed class ManualAllocationsController : ControllerBase
    {
        private readonly IManualAllocationService _service;

        public ManualAllocationsController(IManualAllocationService service)
        {
            _service = service;
        }

        /// Preview (no escribe DB). Devuelve Warnings si algo impediría ejecutar.
        [HttpPost("preview")]
        [ProducesResponseType(typeof(ManualAllocationPreviewDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Preview(
            [FromRoute] int customerId,
            [FromBody] ManualAllocationExecuteDTO request,
            CancellationToken ct = default)
        {
            if (request is null) return BadRequest();
            if (request.CustomerId != customerId)
                return Problem(title: "CustomerId mismatch",
                    detail: $"El body tiene CustomerId={request.CustomerId} pero la ruta es {customerId}.",
                    statusCode: StatusCodes.Status400BadRequest);

            var res = await _service.PreviewAsync(request, ct);
            return Ok(res);
        }

        /// Ejecuta solo si el preview no tiene Warnings (exact cover por débito).
        [HttpPost("execute")]
        [ProducesResponseType(typeof(ManualAllocationPreviewDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Execute(
            [FromRoute] int customerId,
            [FromBody] ManualAllocationExecuteDTO request,
            CancellationToken ct = default)
        {
            if (request is null) return BadRequest();
            if (request.CustomerId != customerId)
                return Problem(title: "CustomerId mismatch",
                    detail: $"El body tiene CustomerId={request.CustomerId} pero la ruta es {customerId}.",
                    statusCode: StatusCodes.Status400BadRequest);

            var res = await _service.ExecuteAsync(request, ct);

            // Si hay Warnings, no se ejecutó nada (exact cover incumplido o sin disponible).
            if (!res.CanExecute)
                return BadRequest(new
                {
                    message = "No se ejecutó la imputación. Revise los Warnings.",
                    res.Warnings
                });

            return Ok(res);
        }
    }
}
