using CatalogService.Business.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class StockServiceClient : IStockServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _stockBaseUrl;

        public StockServiceClient(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _stockBaseUrl = configuration["GatewayService:StockBaseUrl"];
        }

        public async Task<bool> HasStockAsync(int articleId)
        {
            var client = CreateAuthorizedClient();

            var response = await client.GetFromJsonAsync<decimal>($"{_stockBaseUrl}{articleId}/summary");

            if (response != 0) return true;
            return false;

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
