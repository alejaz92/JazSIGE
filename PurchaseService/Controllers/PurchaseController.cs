using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;
using PurchaseService.Business.Exceptions;
using System.Security.Claims;

namespace PurchaseService.Controllers;

[ApiController]
[Route("api")]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseDTO>>> GetAll()
    {
        var purchases = await _purchaseService.GetAllAsync();
        return Ok(purchases);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<object>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (items, totalCount) = await _purchaseService.GetAllAsync(page, pageSize);
        return Ok(new
        {
            totalCount,
            items
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseDTO>> GetById(int id)
    {
        var purchase = await _purchaseService.GetByIdAsync(id);
        if (purchase == null)
            return NotFound();

        return Ok(purchase);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseCreateDTO dto)
    {
        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userIdHeader, out int userId))
            return Unauthorized(new { error = "Usuario no autenticado correctamente" });

        try
        {
            var id = await _purchaseService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (PartialSuccessException ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    [HttpPost("{id}/retry-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RetryStockUpdate(int id)
    {
        //if (!IsAdmin())
        //    return Forbid();

        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userIdHeader, out int userId))
            return Unauthorized(new { error = "Usuario no autenticado correctamente" });

        try
        {
            await _purchaseService.RetryStockUpdateAsync(id, userId);
            return Ok(new { message = "Stock actualizado correctamente." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
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

    [HttpPost("retry-all-pending-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RetryAllPendingStock()
    {
        //if (!IsAdmin())
        //    return Forbid();

        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userIdHeader, out int userId))
            return Unauthorized(new { error = "Usuario no autenticado correctamente" });

        try
        {
            var updatedCount = await _purchaseService.RetryAllPendingStockAsync(userId);
            return Ok(new { message = $"Se actualizaron correctamente {updatedCount} compras." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    [HttpGet("pending-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PurchaseDTO>>> GetPendingStock()
    {
        //if (!IsAdmin())
        //    return Forbid();

        try
        {
            var purchases = await _purchaseService.GetPendingStockAsync();
            return Ok(purchases);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    //private bool IsAdmin()
    //{
    //    var roleClaim = User.FindFirst(ClaimTypes.Role);
    //    return roleClaim != null && roleClaim.Value == "Admin";
    //}
}
