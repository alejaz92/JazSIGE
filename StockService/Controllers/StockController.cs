using Microsoft.AspNetCore.Mvc;
using StockService.Business.Interfaces;
using StockService.Business.Models;
using StockService.Business.Models.CommitedStock;
using System.Security.Claims;

namespace StockService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly IPendingStockService _pendingStockService;
    private readonly ICommitedStockService _commitedStockService;
    private readonly IEnumService _enumService;

    public StockController(
        IStockService stockService, 
        IPendingStockService pendingStockService,
        ICommitedStockService commitedStockService,
        IEnumService enumService)
    {
        _stockService = stockService;
        _pendingStockService = pendingStockService;
        _commitedStockService = commitedStockService;
        _enumService = enumService;
    }

    [HttpPost("movement")]
    public async Task<ActionResult<List<DispatchStockDetailDTO>>> RegisterMovement([FromBody] StockMovementCreateDTO dto)
    {
        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userIdHeader, out int userId))
            return Unauthorized(new { error = "Usuario no autenticado correctamente" });

        try
        {
            var result = await _stockService.RegisterMovementAsync(dto, userId);
            return Ok(result);
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


    [HttpGet("{articleId}/warehouse/{warehouseId}")]
    public async Task<ActionResult<decimal>> GetStock(int articleId, int warehouseId)
    {
        try
        {
            var quantity = await _stockService.GetStockAsync(articleId, warehouseId);
            return Ok(quantity);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    [HttpGet("{articleId}/summary")]
    public async Task<ActionResult<decimal>> GetStockSummaryByArticle(int articleId)
    {
        try
        {
            var total = await _stockService.GetStockSummaryAsync(articleId);
            return Ok(total);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    [HttpGet("{warehouseId}/warehouse/summary")]
    public async Task<ActionResult<decimal>> GetStockSummaryByWarehouse(int warehouseId)
    {
        try
        {
            var total = await _stockService.GetStockSummaryByWarehouseAsync(warehouseId);
            return Ok(total);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }


    [HttpGet("{articleId}/movements")]
    public async Task<ActionResult<PaginatedResultDTO<StockMovementDTO>>> GetMovementsByArticle(
    int articleId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _stockService.GetMovementsByArticleAsync(articleId, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    [HttpGet("movement-types")]
    public ActionResult<IEnumerable<EnumDTO>> GetMovementTypes()
    {
        try
        {
            return Ok(_enumService.GetStockMovementTypes());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    [HttpGet("{articleId}")]
    public async Task<ActionResult<IEnumerable<StockDTO>>> GetStockByArticle(int articleId)
    {
        try
        {
            var stockList = await _stockService.GetStockByArticleAsync(articleId);
            return Ok(stockList);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    [HttpPost("pending-entry")]
    public async Task<IActionResult> CreatePendingEntry([FromBody] PendingStockEntryCreateDTO dto)
    {
        try
        {
            await _pendingStockService.AddAsync(dto);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    [HttpPost("commited-entry")]
    public async Task<ActionResult> CreateCommitedEntry([FromBody] CommitedStockEntryCreateDTO dto)
    {
        try
        {
            await _commitedStockService.AddAsync(dto);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    [HttpGet("pending-entry/{purchaseId}")]
    public async Task<ActionResult<List<PendingStockEntryDTO>>> GetPendingByPurchase(int purchaseId)
    {
        var result = await _pendingStockService.GetByPurchaseIdAsync(purchaseId);
        return Ok(result);
    }

    [HttpPost("pending-entry/register")]
    public async Task<IActionResult> RegisterPendingStock([FromBody] RegisterPendingStockInputDTO dto)
    {
        if (!HttpContext.Request.Headers.TryGetValue("X-UserId", out var userIdHeader) ||
            !int.TryParse(userIdHeader, out var userId))
        {
            return Unauthorized("User ID is missing or invalid.");
        }

        await _pendingStockService.RegisterPendingStockAsync(dto, userId);
        return Ok();
    }

    [HttpPost("commited-entry/register")]
    public async Task<IActionResult> RegisterCommitedStock([FromBody] RegisterCommitedStockInputDTO dto)
    {
        if (!HttpContext.Request.Headers.TryGetValue("X-UserId", out var userIdHeader) ||
            !int.TryParse(userIdHeader, out var userId))
        {
            return Unauthorized("User ID is missing or invalid.");
        }
        try
        {
            var output = await _commitedStockService.RegisterCommitedStockAsync(dto, userId);
            return Ok(output);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    // get pending stock by article
    [HttpGet("pending/{articleId}")]
    public async Task<ActionResult<decimal>> GetPendingStockByArticle(int articleId)
    {
        try
        {
            var total = await _pendingStockService.GetPendingStockByArticleAsync(articleId);
            return Ok(total);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    // get commited stock by article id
    [HttpGet("commited/{articleId}")]
    public async Task<ActionResult<CommitedStockSummaryByArticleDTO>> GetCommitedStockByArticle(int articleId)
    {
        try
        {
            var result = await _commitedStockService.GetTotalCommitedStockByArticleIdAsync(articleId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    //get available stock for sale by article id
    [HttpGet("available/{articleId}")]
    public async Task<ActionResult<decimal>> GetAvailableStockForSale(int articleId)
    {
        try
        {
            var currentStock = await _stockService.GetStockSummaryAsync(articleId);
            var pendingStock = await _pendingStockService.GetPendingStockByArticleAsync(articleId);
            var commitedStock = await _commitedStockService.GetTotalCommitedStockByArticleIdAsync(articleId);
            return Ok(currentStock + pendingStock - commitedStock.Total);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

}
