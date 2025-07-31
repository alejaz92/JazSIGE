using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;

namespace SalesService.Business.Services.Clients
{
    public class FiscalServiceClient : IFiscalServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _fiscalBaseUrl;

        public FiscalServiceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _fiscalBaseUrl = configuration["GatewayService:FiscalBaseUrl"]!;
        }

        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            return client;
        }

        public async Task<FiscalDocumentResponseDTO> CreateInvoiceAsync(FiscalDocumentCreateDTO dto)
        {
            var client = CreateAuthorizedClient();
            var response = await client.PostAsJsonAsync($"{_fiscalBaseUrl}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error creating invoice: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<FiscalDocumentResponseDTO>();
            return result!;
        }
    }
}
