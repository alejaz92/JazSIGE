using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models.Clients;
using System.Diagnostics.Eventing.Reader;

namespace PurchaseService.Business.Services
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
            var url = $"{_accountingBaseUrl.TrimEnd('/')}/external-documents";
            var resp = await client.PostAsJsonAsync(url, dto, ct);
            resp.EnsureSuccessStatusCode();
        }

        // void document endpoint, receives  LedgerDocumentKind kind, int externalRefId, [FromQuery] PartyType partyType, CancellationToken ct
        public async Task VoidExternalAsync(string ledgerDocumentKind, int externalRefId, string partyType, CancellationToken ct = default)
        {
            var client = CreateAuthorizedClient();
            var url = $"{_accountingBaseUrl.TrimEnd('/')}/external-documents/void/{ledgerDocumentKind}/{externalRefId}?partyType={partyType}";
            var resp = await client.PutAsync(url, null, ct);
            resp.EnsureSuccessStatusCode();
        }

    }
}
