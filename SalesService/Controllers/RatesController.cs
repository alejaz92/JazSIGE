using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesService.Business.Interfaces;

namespace SalesService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RatesController : ControllerBase
    {
        private readonly IRatesService _rates;

        public RatesController(IRatesService rates)
        {
            _rates = rates;
        }

        [HttpGet("usdars/oficial")]
        public async Task<IActionResult> GetUsdArsOficial()
        {
            try
            {
                var result = await _rates.GetUsdArsOficialAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
