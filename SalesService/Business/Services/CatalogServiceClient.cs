using SalesService.Business.Interfaces;
using SalesService.Business.Models;

namespace SalesService.Business.Services
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

        public async Task<IEnumerable<PriceListDTO>> GetPriceLists()
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetFromJsonAsync<IEnumerable<PriceListDTO>>($"{_catalogBaseUrl}PriceList");
            return response ?? Enumerable.Empty<PriceListDTO>();
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
