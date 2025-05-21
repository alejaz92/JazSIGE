using CompanyService.Infrastructure.Models;

namespace CompanyService.Infrastructure.Interfaces
{
    public interface ICompanyInfoRepository
    {
        Task<CompanyInfo?> GetAsync();
        Task UpdateAsync(CompanyInfo company);
        Task SaveChangesAsync();
    }
}
