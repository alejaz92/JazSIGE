using Microsoft.AspNetCore.Mvc;
using StockService.Business.Interfaces;
using StockService.Business.Models;

namespace StockService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    // POST: api/stock/movement
    [HttpPost("movement")]
    public async Task<IActionResult> RegisterMovement([FromBody] StockMovementDTO dto)
    {
        if (!Request.Headers.TryGetValue("X-UserId", out var userIdHeader) || !int.TryParse(userIdHeader, out int userId))
            return Unauthorized("Missing or invalid X-UserId header");

        await _stockService.RegisterMovementAsync(dto, userId);
        return Ok();
    }

    // GET: api/stock/{articleId}/warehouse/{warehouseId}
    [HttpGet("{articleId}/warehouse/{warehouseId}")]
    public async Task<ActionResult<decimal>> GetStock(int articleId, int warehouseId)
    {
        var quantity = await _stockService.GetStockAsync(articleId, warehouseId);
        return Ok(quantity);
    }

    // GET: api/stock/{articleId}/summary
    [HttpGet("{articleId}/summary")]
    public async Task<ActionResult<decimal>> GetStockSummary(int articleId)
    {
        var total = await _stockService.GetStockSummaryAsync(articleId);
        return Ok(total);
    }

    // GET: api/stock/{articleId}/movements?page=1&pageSize=20
    [HttpGet("{articleId}/movements")]
    public async Task<ActionResult<IEnumerable<StockMovementDetailDTO>>> GetMovementsByArticle(
        int articleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _stockService.GetMovementsByArticleAsync(articleId, page, pageSize);
        return Ok(result);
    }
}
