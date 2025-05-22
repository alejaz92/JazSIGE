using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Article_PriceLists;
using SalesService.Business.Models.SalesQuote;

namespace SalesService.Business.Services.Clients
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

        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            return client;
        }

        public async Task<IEnumerable<PriceListDTO>> GetPriceLists()
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetFromJsonAsync<IEnumerable<PriceListDTO>>($"{_catalogBaseUrl}PriceList");
            return response ?? Enumerable.Empty<PriceListDTO>();
        }


        public async Task<CustomerDTO?> GetCustomerByIdAsync(int customerId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"{_catalogBaseUrl}Customer/{customerId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CustomerDTO>();
        }

        public async Task<TransportDTO?> GetTransportByIdAsync(int transportId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"{_catalogBaseUrl}Transport/{transportId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<TransportDTO>();
        }

        public async Task<PriceListDTO?> GetPriceListByIdAsync(int priceListId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"{_catalogBaseUrl}PriceList/{priceListId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PriceListDTO>();
        }

        public async Task<ArticleDTO?> GetArticleByIdAsync(int articleId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"{_catalogBaseUrl}Article/{articleId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ArticleDTO>();
        }
    }
}
