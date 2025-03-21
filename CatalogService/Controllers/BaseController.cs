using CatalogService.Business.Interfaces;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController<T, TDto, TCreateDto> : ControllerBase
        where T : class
    {
        protected readonly IGenericService<T, TDto, TCreateDto> _service;

        public BaseController(IGenericService<T, TDto, TCreateDto> service)
        {
            _service = service;
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var dtos = await _service.GetAllAsync();
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create(TCreateDto model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var validationError = await _service.ValidateBeforeSave(model);
            if (validationError != null) return BadRequest(validationError);

            var createdEntity = await _service.CreateAsync(model);
            return Ok(createdEntity);
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(int id, TCreateDto model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var validationError = await _service.ValidateBeforeSave(model);
            if (validationError != null) return BadRequest(validationError);

            var updatedDto = await _service.UpdateAsync(id, model);
            if (updatedDto == null)
                return NotFound();

            return Ok(updatedDto);
        }

        [HttpPut("{id}/status")]
        public virtual async Task<IActionResult> UpdateStatus(int id, bool isActive)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();
            var success = await _service.UpdateStatusAsync(id, isActive);
            if (!success)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
