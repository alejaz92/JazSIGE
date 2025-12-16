using CompanyService.Business.Interfaces;
using CompanyService.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompanyService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyInfoController : ControllerBase
    {
        private readonly ICompanyInfoService _service;

        public CompanyInfoController(ICompanyInfoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<CompanyInfoDTO>> Get()
        {
            var companyInfo = await _service.GetAsync();
            if (companyInfo == null) return NotFound("Company info not found");
            return Ok(companyInfo);
        }

        [HttpGet("fiscal-settings")]
        public async Task<ActionResult<CompanyFiscalSettingsDTO>> GetFiscalSettings()
        {
            var settings = await _service.GetFiscalSettingsAsync();
            if (settings == null) return NotFound("Company info not found");
            return Ok(settings);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] CompanyInfoUpdateDTO dto)
        {
            if (dto == null) return BadRequest("Invalid data");
            try
            {
                await _service.UpdateAsync(dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("logo-url")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLogoUrl([FromBody] CompanyLogoUpdateDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.LogoUrl))
                return BadRequest("Invalid logo URL.");

            try
            {
                await _service.UpdateLogoUrlAsync(dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
