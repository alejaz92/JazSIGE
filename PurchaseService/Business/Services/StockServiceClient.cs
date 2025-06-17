using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;

namespace PurchaseService.Business.Services
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

        public async Task RegisterPurchaseMovementsAsync(int userId, int warehouseId, List<(int articleId, decimal quantity)> items, string reference, int? dispatchId)
        {
            var client = _httpClientFactory.CreateClient();

            // Agregar token Authorization
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);

            // Agregar X-UserId
            client.DefaultRequestHeaders.Add("X-UserId", userId.ToString());

            foreach (var item in items)
            {
                var dto = new StockMovementCreateDTO
                {
                    ArticleId = item.articleId,
                    Quantity = item.quantity,
                    MovementType = 1,
                    FromWarehouseId = null,
                    ToWarehouseId = warehouseId,
                    Reference = reference ,
                    DispatchId = dispatchId
                };


                var url = $"{_stockBaseUrl.TrimEnd('/')}/movement";
                var response = await client.PostAsJsonAsync(url, dto);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"➡️ StockService responded: {(int)response.StatusCode} {response.StatusCode}");
                Console.WriteLine($"📝 Body: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"StockService error: {response.StatusCode} - {responseContent}");
                }
            }
        }

        public async Task RegisterPendingStockAsync(PendingStockEntryCreateDTO dto)
        {
            var client = _httpClientFactory.CreateClient();

            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);

            var url = $"{_stockBaseUrl.TrimEnd('/')}/pending-entry";
            var response = await client.PostAsJsonAsync(url, dto);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"StockService error: {response.StatusCode} - {content}");
            }
        }

    }
}
