using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using Microsoft.AspNetCore.Mvc;
using static AccountingService.Business.Models.Ledger.CreditItemsAndApplyDTO;

namespace AccountingService.Controllers
{
    [ApiController]
    [Route("api/invoices/{invoiceId:int}")]
    public sealed class InvoiceCreditsController : ControllerBase
    {
        private readonly IInvoiceCreditApplicationService _service;
        public InvoiceCreditsController(IInvoiceCreditApplicationService service) => _service = service;

        [HttpPost("apply-credits")]
        [ProducesResponseType(typeof(ApplyCreditsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApplyCredits(
            [FromRoute] int invoiceId,
            [FromBody] ApplyCreditsRequest request,
            CancellationToken ct = default)
        {
            if (request is null) return BadRequest();
            if (request.InvoiceId != invoiceId)
                return Problem(title: "InvoiceId mismatch",
                    detail: $"Body.InvoiceId={request.InvoiceId} difiere de ruta={invoiceId}",
                    statusCode: StatusCodes.Status400BadRequest);

            try
            {
                var res = await _service.ApplyAsync(request, ct);
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(title: "No se pudo aplicar créditos", detail: ex.Message, statusCode: 400);
            }
        }
    }
}
