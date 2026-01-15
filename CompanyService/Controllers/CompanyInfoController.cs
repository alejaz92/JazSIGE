using CompanyService.Business.Interfaces;
using CompanyService.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompanyService.Controllers
{
    /// <summary>
    /// Controller for managing company information
    /// Provides endpoints to retrieve and update company data used for invoicing and business configurations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyInfoController : ControllerBase
    {
        private readonly ICompanyInfoService _service;

        /// <summary>
        /// Initializes a new instance of the CompanyInfoController
        /// </summary>
        /// <param name="service">Company info service instance</param>
        public CompanyInfoController(ICompanyInfoService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets the complete company information
        /// </summary>
        /// <returns>Company information DTO containing all company details</returns>
        /// <response code="200">Returns the company information</response>
        /// <response code="404">Company information not found</response>
        [HttpGet]
        [ProducesResponseType(typeof(CompanyInfoDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CompanyInfoDTO>> Get()
        {
            var companyInfo = await _service.GetAsync();
            if (companyInfo == null) return NotFound("Company info not found");
            return Ok(companyInfo);
        }

        /// <summary>
        /// Gets the fiscal settings for the company (ARCA configuration)
        /// </summary>
        /// <returns>Fiscal settings DTO containing ARCA configuration</returns>
        /// <response code="200">Returns the fiscal settings</response>
        /// <response code="404">Company information not found</response>
        [HttpGet("fiscal-settings")]
        [ProducesResponseType(typeof(CompanyFiscalSettingsDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CompanyFiscalSettingsDTO>> GetFiscalSettings()
        {
            var settings = await _service.GetFiscalSettingsAsync();
            if (settings == null) return NotFound("Company info not found");
            return Ok(settings);
        }

        /// <summary>
        /// Updates the company information
        /// Requires Admin role authorization
        /// </summary>
        /// <param name="dto">Company information update DTO</param>
        /// <returns>No content if update successful</returns>
        /// <response code="204">Update successful</response>
        /// <response code="400">Validation failed or invalid data</response>
        /// <response code="401">Unauthorized - JWT token missing or invalid</response>
        /// <response code="403">Forbidden - User does not have Admin role</response>
        /// <response code="404">Company info or related entities not found</response>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] CompanyInfoUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.UpdateAsync(dto);
            return NoContent();
        }

        /// <summary>
        /// Updates only the company logo URL
        /// Requires Admin role authorization
        /// </summary>
        /// <param name="dto">Logo URL update DTO</param>
        /// <returns>No content if update successful</returns>
        /// <response code="204">Update successful</response>
        /// <response code="400">Validation failed or invalid URL</response>
        /// <response code="401">Unauthorized - JWT token missing or invalid</response>
        /// <response code="403">Forbidden - User does not have Admin role</response>
        /// <response code="404">Company info not found</response>
        [HttpPut("logo-url")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLogoUrl([FromBody] CompanyLogoUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.UpdateLogoUrlAsync(dto);
            return NoContent();
        }
    }
}
