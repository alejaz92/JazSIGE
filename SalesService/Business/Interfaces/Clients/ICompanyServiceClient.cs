using SalesService.Business.Models.SalesQuote;

namespace SalesService.Business.Interfaces.Clients
{
    public interface ICompanyServiceClient
    {
        Task<CompanyInfoDTO?> GetCompanyInfoAsync();
    }
}
