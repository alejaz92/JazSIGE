using AccountingService.Business.Models.Clients;

namespace AccountingService.Business.Interfaces.Clients
{
    public interface ICompanyServiceClient
    {
        Task<CompanyInfoDTO?> GetCompanyInfoAsync();
    }
}
