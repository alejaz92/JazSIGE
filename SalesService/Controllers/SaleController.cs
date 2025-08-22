using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesService.Business.Interfaces;
using SalesService.Business.Models.Clients;
using SalesService.Business.Models.DeliveryNote;
using SalesService.Business.Models.Sale;

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

        [HttpGet("{saleId}/invoice/detail")]
        public async Task<ActionResult<InvoiceDetailDTO>> GetInvoiceDetail(int saleId)
        {
            try
            {
                var result = await _saleService.GetInvoiceDetailAsync(saleId);
                if (result == null)
                    return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
