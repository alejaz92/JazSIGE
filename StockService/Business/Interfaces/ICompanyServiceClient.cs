using StockService.Business.Models.Clients;

namespace StockService.Business.Interfaces
{
    public interface ICompanyServiceClient
    {
        Task<CompanyInfoDTO?> GetCompanyInfoAsync();
    }
}
