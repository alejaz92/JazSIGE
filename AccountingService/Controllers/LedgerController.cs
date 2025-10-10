using AccountingService.Business.Interfaces;
using AccountingService.Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Controllers
{
    [Route("api/accounting/{partyType:enum}/{partyId:int}")]
    [ApiController]
    public class LedgerController : ControllerBase
    {
        private readonly ILedgerQueryService _ledgerQueryService;
        private readonly ILogger<LedgerController> _logger;

        public LedgerController(ILedgerQueryService ledgerQueryService, ILogger<LedgerController> logger)
        {
            _ledgerQueryService = ledgerQueryService;
            _logger = logger;
        }

        [HttpGet("balances")]
        public async Task<ActionResult<BalancesDTO>> GetBalances(PartyType partyType, int partyId, CancellationToken ct)
        {
            try
            {
                return Ok(await _ledgerQueryService.GetBalancesAsync(partyType, partyId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balances for {partyType} {partyId}", partyType, partyId);
                return StatusCode(500, new { message = "Error retrieving balances." });
            }
        }

        [HttpGet("ledger")]
        public async Task<ActionResult<PagedResult<LedgerDocumentDTO>>> GetLedger(
        PartyType partyType, int partyId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] LedgerDocumentKind? kind,
        [FromQuery] DocumentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        {
            try
            {
                return Ok(await _ledgerQueryService.GetLedgerAsync(partyType, partyId, from, to, kind, status, page, pageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ledger for {partyType} {partyId}", partyType, partyId);
                return StatusCode(500, new { message = "Error retrieving ledger." });
            }
        }

        [HttpGet("selectables")]
        public async Task<ActionResult<SelectablesDTO>> GetSelectables(PartyType partyType, int partyId, CancellationToken ct)
        {
            try
            {
                return Ok(await _ledgerQueryService.GetSelectablesForReceiptAsync(partyType, partyId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting selectables for {partyType} {partyId}", partyType, partyId);
                return StatusCode(500, new { message = "Error retrieving selectables." });
            }
        }

        [HttpGet("receipt-credits")]
        public async Task<ActionResult<List<SimpleDocDTO>>> GetReceiptCredits(PartyType partyType, int partyId, CancellationToken ct)
        {
            try
            {
                return Ok(await _ledgerQueryService.GetReceiptCreditsAsync(partyType, partyId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting receipt credits for {partyType} {partyId}", partyType, partyId);
                return StatusCode(500, new { message = "Error retrieving receipt credits." });
            }
        }
    }
}
