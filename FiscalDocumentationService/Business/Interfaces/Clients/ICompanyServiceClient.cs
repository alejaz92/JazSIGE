using FiscalDocumentationService.Business.Models.Clients;

namespace FiscalDocumentationService.Business.Interfaces.Clients
{
    public interface ICompanyServiceClient
    {
        Task<CompanyFiscalSettingsDTO?> GetCompanyFiscalSettingsAsync();
    }
}
