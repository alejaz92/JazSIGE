using AccountingService.Business.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Controllers
{
    [Route("api/external-documents")]
    [ApiController]
    public class ExternalDocumentsController : ControllerBase
    {
        private readonly IExternalDocumentIngestionService _ingestionService;
        private readonly ILogger<ExternalDocumentsController> _logger;
        public ExternalDocumentsController(IExternalDocumentIngestionService ingestionService, ILogger<ExternalDocumentsController> logger)
        {
            _ingestionService = ingestionService;
            _logger = logger;
        }

        public record UpsertExternalDocRequest(
            PartyType PartyType,
            int PartyId,
            LedgerDocumentKind Kind,
            int ExternalRefId,
            string ExternalRefNumber,
            DateTime DocumentDate,
            decimal AmountARS,
            string Currency = "ARS",
            decimal FxRate = 1m
        );

        [HttpPost]
        public async Task<ActionResult<int>> Upsert([FromBody] UpsertExternalDocRequest req, CancellationToken ct)
        {
            try
            {
                if (req.Kind == LedgerDocumentKind.Receipt)
                    return BadRequest("Receipts are local documents; not allowed here.");

                var id = await _ingestionService.UpsertFiscalDocumentAsync(
                    req.PartyType,
                    req.PartyId,
                    req.Kind,
                    req.ExternalRefId,
                    req.ExternalRefNumber,
                    req.DocumentDate,
                    req.AmountARS,
                    req.Currency,
                    req.FxRate
                );
                return Ok(id);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ingesting external document");
                return StatusCode(500, new { message = "Unexpected error while ingesting document." });
            }
        }

        // endpoint to void external documents
        [HttpPut("void/{kind}/{externalRefId}")]
        public async Task<ActionResult> VoidExternalDocument(
            LedgerDocumentKind kind,
            int externalRefId,
            [FromQuery] PartyType partyType,
            CancellationToken ct)
        {
            try
            {
                if (kind == LedgerDocumentKind.Receipt)
                    return BadRequest("Receipts are local documents; cannot be voided here.");
                await _ingestionService.CancelFiscalDocumentAsync(partyType, externalRefId, kind);
                
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voiding external document");
                return StatusCode(500, new { message = "Unexpected error while voiding document." });
            }
        }

    }
}
