using FiscalDocumentationService.Business.Models.Arca;
using FiscalDocumentationService.Infrastructure.Models;

namespace FiscalDocumentationService.Business.Interfaces.Clients
{
    public interface IArcaServiceClient
    {
        Task<ArcaResponseDTO> AuthorizeAsync(ArcaRequestDTO request);
    }
}
