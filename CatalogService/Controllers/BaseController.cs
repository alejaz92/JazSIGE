using CatalogService.Business.Interfaces;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

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
            try
            {
                var dtos = await _service.GetAllAsync();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
            // Podés loguear si tenés un ILogger
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Error ocurred.",
                detail = ex.Message
            });
            }
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(int id)
        {
            try
            {
                var dto = await _service.GetByIdAsync(id);
                if (dto == null)
                    return NotFound();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                // Podés loguear si tenés un ILogger
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error ocurred.",
                    detail = ex.Message
                });
            }
        }

        // get by ids
        [HttpPost("batch")]
        public virtual async Task<IActionResult> GetByIds([FromBody] List<int> ids)
        {
            try
            {
                var dtos = await _service.GetByIdsAsync(ids);
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                // Podés loguear si tenés un ILogger
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error ocurred.",
                    detail = ex.Message
                });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create(TCreateDto model)
        {
            try
            {
                var createdEntity = await _service.CreateAsync(model);
                return Ok(createdEntity);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error ocurred while creating.",
                    detail = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(int id, TCreateDto model)
        {

            try
            {
                var updatedDto = await _service.UpdateAsync(id, model);
                if (updatedDto == null)
                    return NotFound();

                return Ok(updatedDto);
            }
            catch (Exception ex)
            {
                // Podés loguear si tenés un ILogger
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error ocurred while updating.",
                    detail = ex.Message
                });
            }
        }

        [HttpPut("{id}/status")]
        public virtual async Task<IActionResult> UpdateStatus(int id, [FromBody] JsonElement dto)
        {
            try
            {
                if (!dto.TryGetProperty("isActive", out var isActiveElement) ||
                    (isActiveElement.ValueKind != JsonValueKind.True && isActiveElement.ValueKind != JsonValueKind.False))
                {
                    return BadRequest("El campo 'isActive' es obligatorio y debe ser booleano.");
                }

                bool isActive = isActiveElement.GetBoolean();
                var success = await _service.UpdateStatusAsync(id, isActive);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error ocurred while updating status.",
                    detail = ex.Message
                });
            }
        }


        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Error ocurred while deleting.",
                    detail = ex.Message
                });
            }
        }
    }
}
