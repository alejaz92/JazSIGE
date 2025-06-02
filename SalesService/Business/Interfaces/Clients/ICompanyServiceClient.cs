using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface ICompanyServiceClient
    {
        Task<CompanyInfoDTO?> GetCompanyInfoAsync();
    }
}
