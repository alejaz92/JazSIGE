using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;

namespace PurchaseService.Business.Services
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

        public async Task<string?> GetSupplierNameAsync(int supplierId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetFromJsonAsync<SupplierDTO>($"{_catalogBaseUrl}Supplier/{supplierId}");
            return response?.CompanyName;
        }

        public async Task<string?> GetWarehouseNameAsync(int warehouseId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetFromJsonAsync<WarehouseDTO>($"{_catalogBaseUrl}Warehouse/{warehouseId}");
            return response?.Description;
        }

        public async Task<string?> GetArticleNameAsync(int articleId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetFromJsonAsync<ArticleDTO>($"{_catalogBaseUrl}Article/{articleId}");
            return response?.Description;
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
