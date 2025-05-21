using CompanyService.Infrastructure.Data;
using CompanyService.Infrastructure.Interfaces;
using CompanyService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyService.Infrastructure.Repositories
{
    public class CompanyInfoRepository : ICompanyInfoRepository
    {
        private readonly CompanyDbContext _context;

        public CompanyInfoRepository(CompanyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CompanyInfo?> GetAsync()
        {
            return await _context.CompanyInfo.FirstOrDefaultAsync();
        }
        public async Task UpdateAsync(CompanyInfo company)
        {
            _context.CompanyInfo.Update(company);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
