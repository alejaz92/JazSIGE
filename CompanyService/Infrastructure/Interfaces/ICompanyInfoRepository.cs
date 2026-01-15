using CompanyService.Infrastructure.Models;

namespace CompanyService.Infrastructure.Interfaces
{
    /// <summary>
    /// Repository interface for company information data access operations
    /// </summary>
    public interface ICompanyInfoRepository
    {
        /// <summary>
        /// Retrieves the company information entity
        /// </summary>
        /// <returns>CompanyInfo entity, or null if not found</returns>
        Task<CompanyInfo?> GetAsync();
        
        /// <summary>
        /// Marks the company entity as modified for update
        /// </summary>
        /// <param name="company">Company entity to update</param>
        Task UpdateAsync(CompanyInfo company);
        
        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        Task SaveChangesAsync();
    }
}
