using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.Arca;

namespace FiscalDocumentationService.Business.Services.Clients
{
    public class ArcaServiceClient : IArcaServiceClient
    {
        public Task<ArcaResponseDTO> AuthorizeAsync(ArcaRequestDTO request)
        {
            // Simulación simple: todo aprobado
            var cae = GenerateRandomCae();
            var response = new ArcaResponseDTO
            {
                result = "A", // Aprobado
                cae = cae,
                caeExpirationDate = DateTime.Now.AddDays(10).ToString("yyyyMMdd"),
                observations = new List<ArcaObservation>
                {
                    new ArcaObservation
                    {
                        code = 1000,
                        message = "Simulated approval by ARCA dummy client"
                    }
                }
            };

            return Task.FromResult(response);
        }

        private string GenerateRandomCae()
        {
            var random = new Random();
            return random.Next(100000000, 999999999).ToString();
        }
    }
}
