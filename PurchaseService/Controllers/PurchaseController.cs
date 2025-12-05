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

    //[HttpPost]
    //public async Task<IActionResult> Create([FromBody] PurchaseCreateDTO dto)
    //{
    //    var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
    //    if (!int.TryParse(userIdHeader, out int userId))
    //        return Unauthorized(new { error = "Usuario no autenticado correctamente" });

    //    try
    //    {
    //        var purchaseDocumentDTO = await _purchaseService.CreateAsync(dto, userId);
    //        return CreatedAtAction(purchaseDocumentDTO);
    //    }
    //    catch (PartialSuccessException ex)
    //    {
    //        return StatusCode(500, new { error = ex.Message });
    //    }
    //    catch (ArgumentException ex)
    //    {
    //        return BadRequest(new { error = ex.Message });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
    //    }
    //}

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseCreateDTO dto)
    {
        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userIdHeader, out int userId))
            return Unauthorized(new { error = "Usuario no autenticado correctamente" });

        try
        {
            var purchaseDocumentDTO = await _purchaseService.CreateAsync(dto, userId);
            // Devolver 200 OK con el DTO (puede ser null). Si quieres 201 cuando se crea realmente,
            // cambiar a Created(...) sólo cuando purchaseDocumentDTO != null.
            return Ok(purchaseDocumentDTO);
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

    [HttpPost("{id}/register-stock")]
    public async Task<IActionResult> RegisterStockFromPending(int id, [FromBody] RegisterStockInputDTO dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            await _purchaseService.RegisterStockFromPendingAsync(id, dto.WarehouseId, dto.Reference, userId, dto.DispatchId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }

    }

    [HttpGet("article/{articleId}/history")]
    public async Task<ActionResult<IEnumerable<ArticlePurchaseHistoryDTO>>> GetPurchaseHistoryByArticleId(int articleId)
    {
        try
        {
            var history = await _purchaseService.GetPurchaseHistoryByArticleIdAsync(articleId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    [HttpPut("{id}/articles")]
    public async Task<IActionResult> UpdateArticles(int id, [FromBody] IEnumerable<PurchaseArticleUpdateDTO> updates)
    {
        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userIdHeader, out int userId))
            return Unauthorized(new { error = "Invalid or missing user authentication." });

        try
        {
            var result = await _purchaseService.UpdateArticlesAsync(id, updates, userId);
            if (result != null)
                return Ok(result);
            else
                return Ok(new { message = "Articles updated successfully (no conflicts detected)." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }


}
