using AccountingService.Business.Models.Ledger;
using AccountingService.Business.Services.Interfaces;
using AccountingService.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly ILedgerDocumentService _service;

        public DocumentsController(ILedgerDocumentService ledgerDocumentService)
            => _service = ledgerDocumentService;

        [HttpPost]
        [ProducesResponseType(typeof(DocumentResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] DocumentRequestCreateDTO request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(request, ct);
            // Devolvemos 201. Si más adelante querés GET by id, el Location puede apuntar ahí.
            return StatusCode(StatusCodes.Status201Created, result);
        }

         
    }
}
