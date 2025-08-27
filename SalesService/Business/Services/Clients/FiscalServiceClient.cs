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
        public async Task<FiscalDocumentResponseDTO?> GetBySaleIdAsync(int salesOrderId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"{_fiscalBaseUrl}/by-sales-order/{salesOrderId}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error fetching invoice: {errorContent}");
            }
            var result = await response.Content.ReadFromJsonAsync<FiscalDocumentResponseDTO>();
            return result;
        }
        public async Task<FiscalDocumentResponseDTO> CreateCreditNoteAsync(CreditNoteCreateClientDTO dto)
        {
            var client = CreateAuthorizedClient();
            var response = await client.PostAsJsonAsync($"{_fiscalBaseUrl}/credit-notes", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error creating credit note: {errorContent}");
            }
            var result = await response.Content.ReadFromJsonAsync<FiscalDocumentResponseDTO>();
            return result!;
        }
        public async Task<FiscalDocumentResponseDTO> CreateDebitNoteAsync(DebitNoteCreateClientDTO dto)
        {
            var client = CreateAuthorizedClient(); // igual que en CreateInvoiceAsync / CreateCreditNoteAsync
            var response = await client.PostAsJsonAsync($"{_fiscalBaseUrl}/debit-notes", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error creating debit note: {error}");
            }
            var result = await response.Content.ReadFromJsonAsync<FiscalDocumentResponseDTO>();
            return result!;
        }
    }
}
