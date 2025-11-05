using AccountingService.Business.Interfaces;
using AccountingService.Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Controllers
{
    [Route("api/{partyType}/{partyId:int}")]
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
        public async Task<ActionResult<BalancesDTO>> GetBalances(string partyType, int partyId, CancellationToken ct)
        {
            if (!Enum.TryParse<PartyType>(partyType, true, out var parsedPartyType))
                return BadRequest(new { message = "Invalid partyType." });

            try
            {
                return Ok(await _ledgerQueryService.GetBalancesAsync(parsedPartyType, partyId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balances for {partyType} {partyId}", parsedPartyType, partyId);
                return StatusCode(500, new { message = "Error retrieving balances." });
            }
        }

        [HttpGet("ledger")]
        public async Task<ActionResult<PagedResult<LedgerDocumentDTO>>> GetLedger(
            string partyType, int partyId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] LedgerDocumentKind? kind,
            [FromQuery] DocumentStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            if (!Enum.TryParse<PartyType>(partyType, true, out var parsedPartyType))
                return BadRequest(new { message = "Invalid partyType." });

            try
            {
                return Ok(await _ledgerQueryService.GetLedgerAsync(parsedPartyType, partyId, from, to, kind, status, page, pageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ledger for {partyType} {partyId}", parsedPartyType, partyId);
                return StatusCode(500, new { message = "Error retrieving ledger." });
            }
        }

        [HttpGet("selectables")]
        public async Task<ActionResult<SelectablesDTO>> GetSelectables(string partyType, int partyId, CancellationToken ct)
        {
            if (!Enum.TryParse<PartyType>(partyType, true, out var parsedPartyType))
                return BadRequest(new { message = "Invalid partyType." });

            try
            {
                return Ok(await _ledgerQueryService.GetSelectablesForReceiptAsync(parsedPartyType, partyId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting selectables for {partyType} {partyId}", parsedPartyType, partyId);
                return StatusCode(500, new { message = "Error retrieving selectables." });
            }
        }

        [HttpGet("receipt-credits")]
        public async Task<ActionResult<List<SimpleDocDTO>>> GetReceiptCredits(string partyType, int partyId, CancellationToken ct)
        {
            if (!Enum.TryParse<PartyType>(partyType, true, out var parsedPartyType))
                return BadRequest(new { message = "Invalid partyType." });

            try
            {
                return Ok(await _ledgerQueryService.GetReceiptCreditsAsync(parsedPartyType, partyId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting receipt credits for {partyType} {partyId}", parsedPartyType, partyId);
                return StatusCode(500, new { message = "Error retrieving receipt credits." });
            }
        }

        [HttpGet("statement")]
        public async Task<ActionResult<CustomerStatementDTO>> GetStatement(
            string partyType, int partyId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] LedgerDocumentKind? kind,
            [FromQuery] DocumentStatus? status,
            CancellationToken ct = default)
        {
            if (!Enum.TryParse<PartyType>(partyType, true, out var parsedPartyType))
                return BadRequest(new { message = "Invalid partyType." });

            try
            {
                var dto = await _ledgerQueryService.GetStatementAsync(
                    parsedPartyType, partyId, from, to, kind, status, ct);

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statement for {partyType} {partyId}", parsedPartyType, partyId);
                return StatusCode(500, new { message = "Error retrieving statement." });
            }
        }
    }
}
