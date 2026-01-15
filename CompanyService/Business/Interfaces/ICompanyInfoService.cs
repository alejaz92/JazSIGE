using CompanyService.Business.Models;

namespace CompanyService.Business.Interfaces
{
    /// <summary>
    /// Service interface for company information business operations
    /// </summary>
    public interface ICompanyInfoService
    {
        /// <summary>
        /// Retrieves the complete company information
        /// </summary>
        /// <returns>Company information DTO, or null if not found</returns>
        Task<CompanyInfoDTO> GetAsync();
        
        /// <summary>
        /// Retrieves fiscal settings for the company (ARCA configuration)
        /// </summary>
        /// <returns>Fiscal settings DTO, or null if company not found</returns>
        Task<CompanyFiscalSettingsDTO?> GetFiscalSettingsAsync();
        
        /// <summary>
        /// Updates company information
        /// </summary>
        /// <param name="dto">Company information update DTO</param>
        Task UpdateAsync(CompanyInfoUpdateDTO dto);
        
        /// <summary>
        /// Updates only the company logo URL
        /// </summary>
        /// <param name="dto">Logo URL update DTO</param>
        Task UpdateLogoUrlAsync(CompanyLogoUpdateDTO dto);
    }
}
