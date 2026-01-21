using FiscalDocumentationService.Business.Exceptions;
using FiscalDocumentationService.Business.Interfaces;
using FiscalDocumentationService.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static FiscalDocumentationService.Business.Exceptions.FiscalDocumentationException;

namespace FiscalDocumentationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FiscalDocumentController : ControllerBase
    {
        private readonly IFiscalDocumentService _fiscalDocumentService;

        public FiscalDocumentController(IFiscalDocumentService fiscalDocumentService)
        {
            _fiscalDocumentService = fiscalDocumentService;
        }

        [HttpPost]
        public async Task<ActionResult<FiscalDocumentDTO>> Create([FromBody] FiscalDocumentCreateDTO dto)
        {
            try
            {
                var result = await _fiscalDocumentService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (FiscalValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<FiscalDocumentDTO>> GetById(int id)
        {
            var document = await _fiscalDocumentService.GetByIdAsync(id);
            if (document == null)
                return NotFound();

            return Ok(document);
        }

        [HttpGet("by-sales-order/{salesOrderId}")]
        public async Task<ActionResult<FiscalDocumentDTO>> GetBySalesOrderId(int salesOrderId)
        {
            var document = await _fiscalDocumentService.GetBySalesOrderIdAsync(salesOrderId);
            if (document == null)
                return NotFound();

            return Ok(document);
        }

        [HttpGet("credit-notes")]
        public async Task<ActionResult<IEnumerable<FiscalDocumentDTO>>> GetCreditNotes([FromQuery] int saleId)
        {
            if (saleId <= 0) return BadRequest(new { message = "relatedId must be > 0." });
            var list = await _fiscalDocumentService.GetCreditNotesBySaleIdAsync(saleId);
            return Ok(list);
        }

        [HttpGet("debit-notes")]
        public async Task<ActionResult<IEnumerable<FiscalDocumentDTO>>> GetDebitNotes([FromQuery] int saleId)
        {
            if (saleId <= 0) return BadRequest(new { message = "relatedId must be > 0." });
            var list = await _fiscalDocumentService.GetDebitNotesBySaleIdAsync(saleId);
            return Ok(list);
        }

    }
}
