using Microsoft.AspNetCore.Mvc;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;

namespace PurchaseService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DispatchController : ControllerBase
{
    private readonly IDispatchService _dispatchService;

    public DispatchController(IDispatchService dispatchService)
    {
        _dispatchService = dispatchService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DispatchDTO>>> GetAll()
    {
        var dispatches = await _dispatchService.GetAllAsync();
        return Ok(dispatches);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<object>> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (items, totalCount) = await _dispatchService.GetAllAsync(page, pageSize);
        return Ok(new { totalCount, items });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DispatchDTO>> GetById(int id)
    {
        var dispatch = await _dispatchService.GetByIdAsync(id);
        if (dispatch == null) return NotFound();
        return Ok(dispatch);
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DispatchCreateDTO dto, [FromQuery] int purchaseId)
    {
        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userIdHeader, out int userId))
            return Unauthorized(new { error = "Usuario no autenticado correctamente" });
        try
        {
            var id = await _dispatchService.CreateAsync(dto, userId, purchaseId);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            // Handle specific exceptions if needed
            return StatusCode(500, new { error = ex.Message });
        }
    }


}
