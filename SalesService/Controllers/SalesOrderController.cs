using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesService.Business.Interfaces;
using SalesService.Business.Models.SalesOrder;

namespace SalesService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SalesOrderController : ControllerBase
    {
        private readonly ISalesOrderService _salesOrderService;

        public SalesOrderController(ISalesOrderService salesOrderService)
        {
            _salesOrderService = salesOrderService;
        }

        [HttpPost]
        public async Task<ActionResult<SalesOrderDTO>> Create(SalesOrderCreateDTO dto)
        {
            try
            {
                var result = await _salesOrderService.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SalesOrderDTO>> GetById(int id)
        {
            try
            {
                var result = await _salesOrderService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new {message = ex.Message});
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesOrderListDTO>>> GetAll()
        {
            try
            {
                var result = await _salesOrderService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
