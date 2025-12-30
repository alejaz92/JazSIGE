using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.Clients;

namespace FiscalDocumentationService.Business.Services.Clients
{
    public class CompanyServiceClient : ICompanyServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _companyBaseUrl;

        public CompanyServiceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _companyBaseUrl = configuration["GatewayService:CompanyBaseUrl"];
        }

        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            return client;
        }

        public async Task<CompanyFiscalSettingsDTO?> GetCompanyFiscalSettingsAsync()
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"{_companyBaseUrl}fiscal-settings");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CompanyFiscalSettingsDTO>();
        }
    }
}
