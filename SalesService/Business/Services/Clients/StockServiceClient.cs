using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.SalesOrder;

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

        public async Task<List<DispatchStockDetailDTO>> RegisterMovementAsync(StockMovementCreateDTO dto, int userId)
        {
            var client = _httpClientFactory.CreateClient();

            // Enviar token y userId
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);

            client.DefaultRequestHeaders.Add("X-UserId", userId.ToString());

            var response = await client.PostAsJsonAsync($"{_stockBaseUrl.TrimEnd('/')}/movement", dto);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"StockService error: {response.StatusCode} - {responseContent}");

            var breakdown = System.Text.Json.JsonSerializer.Deserialize<List<DispatchStockDetailDTO>>(responseContent,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return breakdown ?? new();
        }
    }
}
