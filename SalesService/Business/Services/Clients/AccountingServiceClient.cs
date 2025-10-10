using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;

namespace SalesService.Business.Services.Clients
{
    public class AccountingServiceClient : IAccountingServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _accountingBaseUrl;

        public AccountingServiceClient(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContext = httpContextAccessor;
            _accountingBaseUrl = configuration["GatewayService:AccountingBaseUrl"]!
                                 ?? throw new InvalidOperationException("Missing GatewayService:AccountingBaseUrl");
        }

        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContext.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            return client;
        }

        public async Task UpsertExternalAsync(AccountingExternalUpsertDTO dto, CancellationToken ct = default)
        {
            var client = CreateAuthorizedClient();
            var url = $"{_accountingBaseUrl.TrimEnd('/')}/api/accounting/external-documents";
            var resp = await client.PostAsJsonAsync(url, dto, ct);
            resp.EnsureSuccessStatusCode();
        }

        public async Task<IReadOnlyList<ReceiptCreditDTO>> GetReceiptCreditsAsync(int partyId, CancellationToken ct = default)
        {
            var client = CreateAuthorizedClient();
            var url = $"{_accountingBaseUrl.TrimEnd('/')}/api/accounting/Customer/{partyId}/receipt-credits";
            var resp = await client.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadFromJsonAsync<List<ReceiptCreditDTO>>(cancellationToken: ct);
            return data ?? new List<ReceiptCreditDTO>();
        }

        public async Task CoverInvoiceWithReceiptsAsync(CoverInvoiceRequest dto, CancellationToken ct = default)
        {
            var client = CreateAuthorizedClient();
            var url = $"{_accountingBaseUrl.TrimEnd('/')}/api/allocations/cover-invoice";
            var resp = await client.PostAsJsonAsync(url, dto, ct);
            resp.EnsureSuccessStatusCode(); // 204 NoContent esperado
        }

    }
}
