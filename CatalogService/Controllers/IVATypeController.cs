using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.IVAType;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IVATypeController : BaseController<IVAType, IVATypeDTO, IVATypeCreateDTO>
    {
        public IVATypeController(IIVATypeService ivaTypeService) : base(ivaTypeService) { }

        [HttpPost]
        public override Task<IActionResult> Create(IVATypeCreateDTO model)
            => Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));

        [HttpPut("{id}")]
        public override Task<IActionResult> Update(int id, IVATypeCreateDTO model)
            => Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));

        [HttpPut("{id}/status")]
        public override Task<IActionResult> UpdateStatus(int id, [FromBody] System.Text.Json.JsonElement dto)
            => Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));

        [HttpDelete("{id}")]
        public override Task<IActionResult> Delete(int id)
            => Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));
    }
}
