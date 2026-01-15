using CompanyService.Infrastructure.Data;
using CompanyService.Infrastructure.Interfaces;
using CompanyService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyService.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for accessing company information data
    /// Provides data access operations for CompanyInfo entity
    /// </summary>
    public class CompanyInfoRepository : ICompanyInfoRepository
    {
        private readonly CompanyDbContext _context;

        /// <summary>
        /// Initializes a new instance of the CompanyInfoRepository
        /// </summary>
        /// <param name="context">Database context instance</param>
        /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
        public CompanyInfoRepository(CompanyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves the company information entity
        /// Note: This service assumes a single company record exists
        /// </summary>
        /// <returns>CompanyInfo entity, or null if not found</returns>
        public async Task<CompanyInfo?> GetAsync()
        {
            return await _context.CompanyInfo.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Marks the company entity as modified for update
        /// Changes are persisted when SaveChangesAsync is called
        /// </summary>
        /// <param name="company">Company entity to update</param>
        /// <exception cref="ArgumentNullException">Thrown when company is null</exception>
        public Task UpdateAsync(CompanyInfo company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            _context.CompanyInfo.Update(company);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
