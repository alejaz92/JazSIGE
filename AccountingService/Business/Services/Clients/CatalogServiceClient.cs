using AccountingService.Business.Interfaces.Clients;
using AccountingService.Business.Models.Clients;

namespace AccountingService.Business.Services.Clients
{
    public class CatalogServiceClient : ICatalogServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _catalogBaseUrl;

        public CatalogServiceClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _catalogBaseUrl = configuration["GatewayService:CatalogBaseUrl"]
                              ?? throw new InvalidOperationException("GatewayService:CatalogBaseUrl is not configured.");
        }

        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            return client;
        }

        public async Task<CustomerDTO?> GetCustomerByIdAsync(int customerId)
        {
            var client = CreateAuthorizedClient();
            var url = $"{_catalogBaseUrl}Customer/{customerId}";
            using var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CustomerDTO>();
        }
    }
}
