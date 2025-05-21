using CompanyService.Business.Interfaces;
using CompanyService.Business.Models;

namespace CompanyService.Business.Services
{
    public class CatalogServiceClient : ICatalogServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _catalogBaseUrl;

        public CatalogServiceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _catalogBaseUrl = configuration["GatewayService:CatalogBaseUrl"];
        }

        public async Task<PostalCodeDTO?> GetPostalCodeByIdAsync(int id)
        {
            var client = CreateAuthorizedClient();
            return await client.GetFromJsonAsync<PostalCodeDTO>($"{_catalogBaseUrl}PostalCode/{id}");
        }

        public async Task<IVATypeDTO?> GetIVATypeByIdAsync(int id)
        {
            var client = CreateAuthorizedClient();
            return await client.GetFromJsonAsync<IVATypeDTO>($"{_catalogBaseUrl}IVAType/{id}");
        }

        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            return client;
        }
    }
}
