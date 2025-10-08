using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using Microsoft.AspNetCore.Mvc;
using static AccountingService.Business.Models.Ledger.CreditItemsAndApplyDTO;

namespace AccountingService.Controllers
{
    [ApiController]
    [Route("api/customers/{customerId:int}/credits")]
    public sealed class CustomerCreditsController : ControllerBase
    {
        private readonly ICreditsQueryService _service;
        public CustomerCreditsController(ICreditsQueryService service) => _service = service;

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<CreditItemDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromRoute] int customerId,
            [FromQuery] decimal? minAmount,
            CancellationToken ct = default)
        {
            var data = await _service.GetAvailableAsync(customerId, minAmount, ct);
            return Ok(data);
        }
    }
}
