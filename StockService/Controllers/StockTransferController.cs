using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockService.Business.Interfaces;
using StockService.Business.Models.StockTransfer;

namespace StockService.Controllers
{
    [Route("api/stock/[controller]")]
    [ApiController]
    public class StockTransferController : ControllerBase
    {
        private readonly IStockTransferService _stockTransferService;

        public StockTransferController(IStockTransferService stockTransferService)
        {
            _stockTransferService = stockTransferService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockTransferCreateDTO dto)
        {
            var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
            if (!int.TryParse(userIdHeader, out int userId))
                return Unauthorized(new { error = "Usuario no autenticado correctamente" });

            try
            {
                var id = await _stockTransferService.CreateAsync(dto, userId);
                return Ok(new { id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StockTransferDTO>> GetById(int id)
        {
            var result = await _stockTransferService.GetByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockTransferDTO>>> GetAll()
        {
            var result = await _stockTransferService.GetAllAsync();
            return Ok(result);
        }
    }
}
