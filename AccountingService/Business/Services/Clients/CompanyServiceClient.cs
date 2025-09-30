using AccountingService.Business.Interfaces.Clients;
using AccountingService.Business.Models.Clients;

namespace AccountingService.Business.Services.Clients
{
    public class CompanyServiceClient : ICompanyServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _companyBaseUrl;

        public CompanyServiceClient(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _companyBaseUrl = configuration["GatewayService:CompanyBaseUrl"]
                              ?? throw new InvalidOperationException("GatewayService:CompanyBaseUrl is not configured.");
        }

        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            return client;
        }

        public async Task<CompanyInfoDTO?> GetCompanyInfoAsync()
        {
            var client = CreateAuthorizedClient();
            var url = $"{_companyBaseUrl}";
            using var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CompanyInfoDTO>();
        }
    }
}
