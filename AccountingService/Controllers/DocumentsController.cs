using AccountingService.Business.Interfaces;
using AccountingService.Business.Models.Ledger;
using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentIntakeService _service;

        public DocumentsController(IDocumentIntakeService service)
        {
            _service = service;
        }

        // Ingesta de documentos fiscales (Invoice/DN/CN)
        [HttpPost("fiscal")]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IngestFiscal([FromBody] FiscalDocumentCreateDTO request, CancellationToken ct = default)
        {
            var result = await _service.IngestFiscalAsync(request, ct);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        // Anulación de documento fiscal por (SourceKind, SourceDocumentId)
        [HttpPost("fiscal/void")]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VoidFiscal([FromBody] VoidFiscalRequest request, CancellationToken ct = default)
        {
            var result = await _service.VoidFiscalAsync(request.SourceKind, request.SourceDocumentId, ct);
            if (result is null) return NotFound();
            return Ok(result);
        }

        public class VoidFiscalRequest
        {
            public SourceKind SourceKind { get; set; }
            public long SourceDocumentId { get; set; }
        }
    }
}
