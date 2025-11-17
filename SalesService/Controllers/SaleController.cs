using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesService.Business.Interfaces;
using SalesService.Business.Models.Clients;
using SalesService.Business.Models.DeliveryNote;
using SalesService.Business.Models.Sale;
using SalesService.Business.Models.Sale.fiscalDocs;
using System.Security.Claims;

namespace SalesService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SaleController : ControllerBase
    {
        private readonly ISaleService _saleService;
        private readonly IDeliveryNoteService _deliveryNoteService;

        public SaleController(
            ISaleService saleService, IDeliveryNoteService deliveryNoteService)
        {
            _saleService = saleService;
            _deliveryNoteService = deliveryNoteService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleDTO>>> GetAll()
        {
            try
            {
                var result = await _saleService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SaleDetailDTO>> GetById(int id)
        {
            try
            {
                var result = await _saleService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<SaleDetailDTO>> Create(SaleCreateDTO dto)
        {
            try
            {
                var result = await _saleService.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("quick")]
        public async Task<ActionResult<QuickSaleResultDTO>> CreateQuick([FromBody] QuickSaleCreateDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int userId = int.Parse(userIdClaim.Value);

                var result = await _saleService.CreateQuickAsync(dto, userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{saleId}/delivery-note")]
        public async Task<ActionResult<DeliveryNoteDTO>> CreateDeliveryNote(int saleId, [FromBody] DeliveryNoteCreateDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized();

                int userId = int.Parse(userIdClaim.Value);

                var result = await _deliveryNoteService.CreateAsync(saleId, dto, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{saleId}/delivery-notes")]
        public async Task<ActionResult<IEnumerable<DeliveryNoteDTO>>> GetDeliveryNotes(int saleId)
        {
            try
            {
                var result = await _deliveryNoteService.GetAllBySaleIdAsync(saleId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("delivery-note/{id}")]
        public async Task<ActionResult<DeliveryNoteDTO>> GetDeliveryNoteById(int id)
        {
            try
            {
                var result = await _deliveryNoteService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{saleId}/invoice")]
        public async Task<ActionResult<FiscalDocumentResponseDTO>> CreateInvoice(int saleId)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized();

                // opcional: podrías usar userId más adelante si querés registrar quién generó la factura
                var result = await _saleService.CreateInvoiceAsync(saleId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{saleId}/invoice")]
        public async Task<ActionResult<InvoiceBasicDTO>> GetInvoice(int saleId)
        {
            try
            {
                var result = await _saleService.GetInvoiceAsync(saleId);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}/invoice/detail")]
        public async Task<ActionResult<InvoiceDetailDTO>> GetInvoiceDetail(int id)
        {
            try
            {
                var result = await _saleService.GetInvoiceDetailAsync(id);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{saleId}/credit-note")]
        public async Task<ActionResult<InvoiceBasicDTO>> CreateCreditNote(int saleId, [FromBody] CreditNoteCreateForSaleDTO dto)
        {
            try
            {
                var result = await _saleService.CreateCreditNoteAsync(saleId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        [HttpPost("{saleId}/debit-note")]
        public async Task<ActionResult<InvoiceBasicDTO>> CreateDebitNote(int saleId, [FromBody] DebitNoteCreateForSaleDTO dto)
        {
            try
            {
                var result = await _saleService.CreateDebitNoteAsync(saleId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // Controllers/SaleController.cs
        [HttpGet("{saleId}/credit-notes")]
        public async Task<ActionResult<IEnumerable<SaleNoteSummaryDTO>>> GetCreditNotes(int saleId)
        {
            var res = await _saleService.GetCreditNotesAsync(saleId);
            return Ok(res);
        }

        [HttpGet("{saleId}/debit-notes")]
        public async Task<ActionResult<IEnumerable<SaleNoteSummaryDTO>>> GetDebitNotes(int saleId)
        {
            var res = await _saleService.GetDebitNotesAsync(saleId);
            return Ok(res);
        }

        [HttpGet("{saleId}/notes")]
        public async Task<ActionResult<IEnumerable<SaleNoteSummaryDTO>>> GetAllNotes(int saleId)
        {
            var res = await _saleService.GetAllNotesAsync(saleId);
            return Ok(res);
        }

        [HttpPost("{saleId}/invoice/{invoiceExternalRefId}/cover")]
        public async Task<IActionResult> CoverInvoice(
            int saleId,
            int invoiceExternalRefId,
            [FromBody] CoverInvoiceRequest request,
            CancellationToken ct = default)
        {
            // asegurar coherencia si el front no mandó el id en el body
            if (request.InvoiceExternalRefId == 0)
                request.InvoiceExternalRefId = invoiceExternalRefId;

            await _saleService.CoverInvoiceWithReceiptsAsync(saleId, request, ct);
            return NoContent(); // 204
        }


        // --------------------------------------------------------------
        // Receives stock warnings from StockService and registers them.
        // --------------------------------------------------------------
        [HttpPost("stock-warnings")]
        public async Task<IActionResult> RegisterStockWarnings([FromBody] IEnumerable<SaleStockWarningInputDTO> warnings)
        {
            if (warnings == null)
            {
                return BadRequest(new { error = "Invalid payload." });
            }

            await _saleService.RegisterStockWarningsAsync(warnings);

            return Ok(new { message = "Stock warnings registered successfully." });
        }
        [HttpPut("{saleId}/resolve-stock-warning")]
        public async Task<ActionResult<SaleResolveStockWarningResultDTO>> ResolveWarning(
            int saleId,
            [FromBody] SaleResolveStockWarningDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var result = await _saleService.ResolveStockWarningAsync(saleId, dto, userId);
            return Ok(result);
        }


    }
}
