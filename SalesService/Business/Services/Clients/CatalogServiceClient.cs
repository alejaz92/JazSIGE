using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Article_PriceLists;
using SalesService.Business.Models.Clients;

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

        public async Task<List<CustomerDTO>> GetAllCustomersAsync()
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetFromJsonAsync<List<CustomerDTO>>($"{_catalogBaseUrl}Customer");
            return response ?? new List<CustomerDTO>();
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
        public async Task<List<ArticleDTO>> GetArticlesByIdsAsync(List<int> articleIds)
        {
            if (articleIds == null || articleIds.Count == 0)
                return new List<ArticleDTO>();

            // validate ids
            if (articleIds.Any(id => id <= 0))
                throw new ArgumentException("Article ids must be positive integers.", nameof(articleIds));

            const int MaxBatchSize = 100; // limit per request to avoid too large payloads
            var client = CreateAuthorizedClient();
            var result = new List<ArticleDTO>();

            // If too many ids, split into chunks and sequentially request (could be parallel if desired)
            for (int i = 0; i < articleIds.Count; i += MaxBatchSize)
            {
                var chunk = articleIds.Skip(i).Take(MaxBatchSize).ToList();
                var response = await client.PostAsJsonAsync($"{_catalogBaseUrl}Article/Batch", chunk);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"CatalogService error when fetching articles batch: {response.StatusCode} - {content}");
                }

                var articles = await response.Content.ReadFromJsonAsync<List<ArticleDTO>>() ?? new List<ArticleDTO>();
                result.AddRange(articles);
            }

            // remove duplicates just in case
            return result.GroupBy(a => a.Id).Select(g => g.First()).ToList();
        }

        public async Task<WarehouseDTO?> GetWarehouseByIdAsync(int warehouseId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"{_catalogBaseUrl}Warehouse/{warehouseId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WarehouseDTO>();
        }

        public async Task<PostalCodeDTO?> GetPostalCodeByIdAsync(int postalCodeId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"{_catalogBaseUrl}PostalCode/{postalCodeId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<PostalCodeDTO>();
        }
    }
}
