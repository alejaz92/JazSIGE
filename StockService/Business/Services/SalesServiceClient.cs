using StockService.Business.Interfaces;
using StockService.Business.Models.Clients;

namespace StockService.Business.Services
{
    public class SalesServiceClient : ISalesServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _SalesBaseUrl;

        public SalesServiceClient(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _SalesBaseUrl = configuration["GatewayService:SaleBaseUrl"];
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        //public async Task<TransportDTO> GetTransportAsync(int transportId)
        //{
        //    var client = _httpClientFactory.CreateClient();
        //    // Extraer el token JWT del contexto HTTP
        //    var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        //    // Agregar el token al encabezado de la solicitud
        //    if (!string.IsNullOrEmpty(token))
        //    {
        //        client.DefaultRequestHeaders.Add("Authorization", token);
        //    }


        //    var response = await client.GetFromJsonAsync<TransportDTO>($"{_SalesBaseUrl}Transport/{transportId}");
        //    return response;
        //}

        public async Task SendStockWarningsAsync(IEnumerable<SaleStockWarningInputDTO> warnings)
        {
            if (warnings == null || !warnings.Any())
                return;

            var client = _httpClientFactory.CreateClient();

            // Attach Authorization header from the incoming request
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);

            var url = $"{_SalesBaseUrl}stock-warnings";

            var response = await client.PostAsJsonAsync(url, warnings);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"SalesService returned an error: {response.StatusCode} - {content}");
            }
        }
    }
}
