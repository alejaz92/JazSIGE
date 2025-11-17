using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;

namespace SalesService.Business.Services.Clients
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

        public async Task RegisterCommitedStockAsync(CommitedStockEntryCreateDTO dto)
        {
            var client = _httpClientFactory.CreateClient();

            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);

            var url = $"{_stockBaseUrl.TrimEnd('/')}/commited-entry";
            var response = await client.PostAsJsonAsync(url, dto);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"StockService error: {response.StatusCode} - {content}");
            }
        }

        // register quick sale stock movement
        public async Task RegisterQuickStockMovementAsync(StockMovementCreateDTO dto)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            var url = $"{_stockBaseUrl.TrimEnd('/')}/movement";
            var response = await client.PostAsJsonAsync(url, dto);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"StockService error: {response.StatusCode} - {content}");
            }
        }

        public async Task<CommitedStockEntryOutputDTO> RegisterCommitedStockConsolidatedAsync(CommitedStockInputDTO dto, int userId)
        {
            var client = _httpClientFactory.CreateClient();

            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);

            client.DefaultRequestHeaders.Add("X-UserId", userId.ToString());

            var url = $"{_stockBaseUrl.TrimEnd('/')}/commited-entry/register";
            var response = await client.PostAsJsonAsync(url, dto);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"StockService error: {response.StatusCode} - {content}");
            }

            return await response.Content.ReadFromJsonAsync<CommitedStockEntryOutputDTO>();
        }

        // get available stock by article
        public async Task<decimal> GetAvailableStockAsync(int articleId)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            var url = $"{_stockBaseUrl.TrimEnd('/')}/available/{articleId}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"StockService error: {response.StatusCode} - {content}");
            }
            return await response.Content.ReadFromJsonAsync<decimal>();


        }

        // commited stock entry update
        public async Task UpdateCommitedStockEntryAsync(CommitedStockEntryUpdateDTO dto)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            var url = $"{_stockBaseUrl.TrimEnd('/')}/commited-entry";
            var response = await client.PutAsJsonAsync(url, dto);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"StockService error: {response.StatusCode} - {content}");
            }

        }
    }
}
