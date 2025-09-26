using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Common;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Controllers
{
    [Route("api/customers/{customerId:int}")]
    [ApiController]
    public class AccountingCustomerController : ControllerBase
    {
        private readonly ICustomerAccountQueryService _service;

        public AccountingCustomerController(ICustomerAccountQueryService service)
        {
            _service = service;
        }

        [HttpGet("ledger")]
        [ProducesResponseType(typeof(PagedResult<LedgerItemDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLedger(
            int customerId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] LedgerDocumentKind? kind,
            [FromQuery] LedgerDocumentStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var result = await _service.GetLedgerAsync(customerId, from, to, kind, status, page, pageSize, ct);
            return Ok(result);
        }

        [HttpGet("pending")]
        [ProducesResponseType(typeof(PagedResult<PendingItemDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPending(
            int customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100,
            CancellationToken ct = default)
        {
            var result = await _service.GetPendingAsync(customerId, page, pageSize, ct);
            return Ok(result);
        }

        [HttpGet("balances")]
        [ProducesResponseType(typeof(BalancesDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBalances(int customerId, CancellationToken ct = default)
        {
            var result = await _service.GetBalancesAsync(customerId, ct);
            return Ok(result);
        }
    }
}
