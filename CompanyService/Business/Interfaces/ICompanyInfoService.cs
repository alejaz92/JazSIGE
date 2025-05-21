using CompanyService.Business.Models;

namespace CompanyService.Business.Interfaces
{
    public interface ICompanyInfoService
    {
        Task<CompanyInfoDTO> GetAsync();
        Task UpdateAsync(CompanyInfoUpdateDTO dto);
        Task UpdateLogoUrlAsync(CompanyLogoUpdateDTO dto);
    }
}
