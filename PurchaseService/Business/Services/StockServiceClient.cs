﻿using PurchaseService.Business.Interfaces;
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

        public async Task RegisterPurchaseMovementsAsync(int userId, int warehouseId, List<(int articleId, decimal quantity)> items)
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
                    MovementType = "Purchase",
                    FromWarehouseId = null,
                    ToWarehouseId = warehouseId,
                    Reference = "Purchase"
                };

                await client.PostAsJsonAsync($"{_stockBaseUrl}stock/movement", dto);
            }
        }
    }
}
