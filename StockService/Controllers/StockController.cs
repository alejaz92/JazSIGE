﻿using Microsoft.AspNetCore.Mvc;
using StockService.Business.Interfaces;
using StockService.Business.Models;
using System.Security.Claims;

namespace StockService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly IPendingStockService _pendingStockService;
    private readonly IEnumService _enumService;

    public StockController(
        IStockService stockService, 
        IPendingStockService pendingStockService,
        IEnumService enumService)
    {
        _stockService = stockService;
        _pendingStockService = pendingStockService;
        _enumService = enumService;
    }

    //[HttpPost("movement")]
    //public async Task<IActionResult> RegisterMovement([FromBody] StockMovementCreateDTO dto)
    //{

    //    var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
    //    if (!int.TryParse(userIdHeader, out int userId))
    //        return Unauthorized(new { error = "Usuario no autenticado correctamente" });

    //    try
    //    {
    //        await _stockService.RegisterMovementAsync(dto, userId);
    //        return Ok();
    //    }
    //    catch (ArgumentException ex)
    //    {
    //        return BadRequest(new { error = ex.Message });
    //    }
    //    catch (InvalidOperationException ex)
    //    {
    //        return BadRequest(new { error = ex.Message });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
    //    }
    //}
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
        await _pendingStockService.AddAsync(dto);
        return Ok();
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

}
