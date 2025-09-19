using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Common;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Controllers
{
    [Route("api/Customers/{customerId:int}")]
    [ApiController]
    public class AccountingCustomerController : ControllerBase
    {
        [HttpGet("ledger")]
        [ProducesResponseType(typeof(PagedResult<CustomerLedgerItemDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLedger(
        [FromServices] ICustomerLedgerQueryService service,
        int customerId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] LedgerDocumentKind? kind,
        [FromQuery] LedgerDocumentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        {
            var result = await service.GetCustomerLedgerAsync(customerId, from, to, kind, status, page, pageSize, ct);
            return Ok(result);
        }


        [HttpGet("pending")]
        [ProducesResponseType(typeof(List<PendingDocumentDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPending(
           [FromServices] ICustomerPendingQueryService service,
           int customerId,
           CancellationToken ct = default)
        {
            var items = await service.GetPendingAsync(customerId, ct);
            return Ok(items);
        }

        [HttpGet("balances")]
        [ProducesResponseType(typeof(CustomerBalancesDTO), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBalances(
        [FromServices] ICustomerBalancesQueryService service,
        int customerId,
        CancellationToken ct)
        {
            // Devuelve { outstandingArs, creditsArs, netBalanceArs }
            var result = await service.GetBalancesAsync(customerId, ct);
            return Ok(result);
        }
    }
}
