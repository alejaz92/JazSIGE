using FiscalDocumentationService.Infrastructure.Models;

namespace FiscalDocumentationService.Business.Interfaces.Clients
{
    public interface IArcaServiceClient
    {
        Task<(string cae, DateTime caeExpiration)> AuthorizeAsync(FiscalDocument document);
    }
}
