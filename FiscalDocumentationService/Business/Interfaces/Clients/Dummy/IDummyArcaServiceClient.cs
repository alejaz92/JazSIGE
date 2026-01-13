using FiscalDocumentationService.Business.Models.Arca;

namespace FiscalDocumentationService.Business.Interfaces.Clients.Dummy
{
    /// <summary>
    /// Dummy service client that simulates ARCA authorization without actually contacting ARCA.
    /// This is useful for development and testing scenarios where ARCA integration is not available.
    /// </summary>
    public interface IDummyArcaServiceClient
    {
        /// <summary>
        /// Simulates an ARCA authorization request, always returning an approved response with a random CAE.
        /// </summary>
        Task<ArcaResponseDTO> AuthorizeAsync(ArcaRequestDTO request);
    }
}
