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

        // Nuevo: Ingesta de documentos fiscales en Accounting
        public async Task IngestFiscalAsync(AccountingFiscalIngestDTO dto, CancellationToken ct = default)
        {
            var client = CreateAuthorizedClient();
            var url = $"{_accountingBaseUrl.TrimEnd('/')}/Documents/fiscal";
            var resp = await client.PostAsJsonAsync(url, dto, ct);
            resp.EnsureSuccessStatusCode();
        }
    }
}
