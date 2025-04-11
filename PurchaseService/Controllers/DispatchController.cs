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
}
