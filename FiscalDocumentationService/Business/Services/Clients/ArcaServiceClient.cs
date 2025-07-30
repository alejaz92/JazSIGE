

using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Infrastructure.Models;

namespace FiscalDocumentationService.Business.Services.Clients
{
    public class ArcaServiceClient : IArcaServiceClient
    {

        // This is a mock implementation of the ARCA service client.
        public Task<(string cae, DateTime caeExpiration)> AuthorizeAsync(FiscalDocument document)
        {
            var cae = GenerateRandomCAE();
            var caeExpiration = DateTime.Now.AddDays(10);

            return Task.FromResult((cae, caeExpiration));
        }

        private string GenerateRandomCAE()
        {
            var random = new Random();
            return random.Next(100000000, 999999999).ToString();
        }
    }
}
