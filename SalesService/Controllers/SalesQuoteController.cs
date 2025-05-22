using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.SalesQuote;

namespace SalesService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SalesQuoteController : ControllerBase
    {
        private readonly ISalesQuoteService _salesQuoteService;

        public SalesQuoteController(ISalesQuoteService salesQuoteService)
        {
            _salesQuoteService = salesQuoteService;
        }

        [HttpPost]
        public async Task<ActionResult<SalesQuoteDTO>> Create(SalesQuoteCreateDTO dto)
        {
            try
            {
                var result = await _salesQuoteService.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesQuoteDTO>> GetById(int id)
        {
            try
            {
                var result = await _salesQuoteService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesQuoteListDTO>>> GetAll()
        {
            try
            {
                var result = await _salesQuoteService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
