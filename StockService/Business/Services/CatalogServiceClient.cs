using StockService.Business.Interfaces;
using StockService.Business.Models.Clients;

namespace StockService.Business.Services
{
    public class CatalogServiceClient : ICatalogServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _catalogBaseUrl;

        public CatalogServiceClient(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _catalogBaseUrl = configuration["GatewayService:CatalogBaseUrl"];
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> ArticleExistsAsync(int articleId)
        {
            var client = _httpClientFactory.CreateClient();

            // Extraer el token JWT del contexto HTTP
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            // Agregar el token al encabezado de la solicitud
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }

            var response = await client.GetAsync($"{_catalogBaseUrl}Article/{articleId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> WarehouseExistsAsync(int warehouseId)
        {

            var client = _httpClientFactory.CreateClient();
            // Extraer el token JWT del contexto HTTP
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            // Agregar el token al encabezado de la solicitud
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }

            var response = await client.GetAsync($"{_catalogBaseUrl}Warehouse/{warehouseId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> GetArticleNameAsync(int articleId)
        {
            var client = _httpClientFactory.CreateClient();
            // Extraer el token JWT del contexto HTTP
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            // Agregar el token al encabezado de la solicitud
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }


            var response = await client.GetFromJsonAsync<ArticleDTO>($"{_catalogBaseUrl}Article/{articleId}");
            return response?.Description;
        }

        public async Task<ArticleDTO> GetArticleAsync(int articleId)
        {
            var client = _httpClientFactory.CreateClient();
            // Extraer el token JWT del contexto HTTP
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            // Agregar el token al encabezado de la solicitud
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }


            var response = await client.GetFromJsonAsync<ArticleDTO>($"{_catalogBaseUrl}Article/{articleId}");
            return response;
        }
        public async Task<string?> GetWarehouseNameAsync(int warehouseId)
        {
            var client = _httpClientFactory.CreateClient();
            // Extraer el token JWT del contexto HTTP
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            // Agregar el token al encabezado de la solicitud
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }


            var response = await client.GetFromJsonAsync<WarehouseDTO>($"{_catalogBaseUrl}Warehouse/{warehouseId}");
            return response?.Description;
        }

        public async Task<string?> GetTransportNameAsync(int transportId)
        {
            TransportDTO response = await GetTransportAsync(transportId);
            return response?.Name;
        }

        public async Task<TransportDTO> GetTransportAsync(int transportId)
        {
            var client = _httpClientFactory.CreateClient();
            // Extraer el token JWT del contexto HTTP
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            // Agregar el token al encabezado de la solicitud
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }


            var response = await client.GetFromJsonAsync<TransportDTO>($"{_catalogBaseUrl}Transport/{transportId}");
            return response;
        }
    }
}
