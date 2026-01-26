using FiscalDocumentationService.Business.Exceptions;
using FiscalDocumentationService.Business.Interfaces;
using FiscalDocumentationService.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static FiscalDocumentationService.Business.Exceptions.FiscalDocumentationException;

namespace FiscalDocumentationService.Controllers
{
    /// <summary>
    /// Fiscal Document API Controller
    /// Handles creation and retrieval of fiscal documents (invoices, credit notes, debit notes)
    /// with ARCA (ex AFIP) integration for electronic invoice authorization.
    /// </summary>
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

        /// <summary>
        /// Creates a new fiscal document (invoice, credit note, or debit note).
        /// For invoices: implements idempotency based on SalesOrderId (returns existing if already created).
        /// For credit/debit notes: requires reference to original invoice fields.
        /// </summary>
        /// <param name="dto">Fiscal document creation request containing items and amounts</param>
        /// <returns>Created fiscal document with CAE authorization details and AFIP QR URL</returns>
        /// <response code="201">Fiscal document successfully created/authorized</response>
        /// <response code="400">Validation error (invalid totals, missing items, reference data, etc.)</response>
        /// <response code="401">Unauthorized - missing or invalid token</response>
        [HttpPost]
        public async Task<ActionResult<FiscalDocumentDTO>> Create([FromBody] FiscalDocumentCreateDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Request body cannot be null." });

            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest(new { message = "At least one item is required in the document." });

            try
            {
                var result = await _fiscalDocumentService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (FiscalValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errorCode = "VALIDATION_ERROR" });
            }
            catch (FiscalConfigurationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = ex.Message, errorCode = "CONFIGURATION_ERROR" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An unexpected error occurred while creating the fiscal document.", errorCode = "INTERNAL_ERROR" });
            }
        }

        /// <summary>
        /// Retrieves a fiscal document by its ID.
        /// </summary>
        /// <param name="id">Fiscal document ID</param>
        /// <returns>Fiscal document details including CAE, authorization status, and items</returns>
        /// <response code="200">Document found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Document not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<FiscalDocumentDTO>> GetById(int id)
        {
            var document = await _fiscalDocumentService.GetByIdAsync(id);
            if (document == null)
                return NotFound();

            return Ok(document);
        }

        /// <summary>
        /// Retrieves the main invoice associated with a sales order.
        /// Used when you need to check if an invoice already exists for a specific order.
        /// </summary>
        /// <param name="salesOrderId">Sales order ID</param>
        /// <returns>Associated fiscal document (invoice)</returns>
        /// <response code="200">Invoice found for order</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">No invoice found for this order</response>
        [HttpGet("by-sales-order/{salesOrderId}")]
        public async Task<ActionResult<FiscalDocumentDTO>> GetBySalesOrderId(int salesOrderId)
        {
            var document = await _fiscalDocumentService.GetBySalesOrderIdAsync(salesOrderId);
            if (document == null)
                return NotFound();

            return Ok(document);
        }

        /// <summary>
        /// Retrieves all credit notes associated with a sale/invoice.
        /// Credit notes reduce the total amount of the original sale.
        /// </summary>
        /// <param name="saleId">Original sale/invoice ID to query credit notes for</param>
        /// <returns>List of credit notes for the specified sale</returns>
        /// <response code="200">Credit notes list (may be empty)</response>
        /// <response code="400">Invalid saleId (must be > 0)</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("credit-notes")]
        public async Task<ActionResult<IEnumerable<FiscalDocumentDTO>>> GetCreditNotes([FromQuery] int saleId)
        {
            if (saleId <= 0) 
                return BadRequest(new { message = "saleId must be > 0." });
            
            var list = await _fiscalDocumentService.GetCreditNotesBySaleIdAsync(saleId);
            return Ok(list);
        }

        /// <summary>
        /// Retrieves all debit notes associated with a sale/invoice.
        /// Debit notes increase the total amount of the original sale (e.g., for additional charges).
        /// </summary>
        /// <param name="saleId">Original sale/invoice ID to query debit notes for</param>
        /// <returns>List of debit notes for the specified sale</returns>
        /// <response code="200">Debit notes list (may be empty)</response>
        /// <response code="400">Invalid saleId (must be > 0)</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("debit-notes")]
        public async Task<ActionResult<IEnumerable<FiscalDocumentDTO>>> GetDebitNotes([FromQuery] int saleId)
        {
            if (saleId <= 0) 
                return BadRequest(new { message = "saleId must be > 0." });
            
            var list = await _fiscalDocumentService.GetDebitNotesBySaleIdAsync(saleId);
            return Ok(list);
        }

    }
}
