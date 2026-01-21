using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.Arca;
using FiscalDocumentationService.Business.Models.Clients;
using FiscalDocumentationService.Business.Options;
using Microsoft.Extensions.Options;
using static FiscalDocumentationService.Business.Exceptions.FiscalDocumentationException;


namespace FiscalDocumentationService.Business.Services.Clients
{
    public class ArcaAuthClient : IArcaAuthClient
    {
        private readonly HttpClient _http;
        private readonly ArcaOptions _options;
        private readonly IArcaAccessTicketCache _cache;

        public ArcaAuthClient(HttpClient http, IOptions<ArcaOptions> options, IArcaAccessTicketCache cache)
        {
            _http = http;
            _options = options.Value;
            _cache = cache;
        }

        public async Task<ArcaAccessTicket> GetAccessTicketAsync(string serviceName)
        {
            // 1) Cache (fast path)
            var cached = _cache.Get(serviceName);
            if (cached != null && cached.IsValid())
                return cached;

            // 2) Lock per serviceName (avoid concurrent TA requests)
            var sem = _cache.GetLock(serviceName);
            await sem.WaitAsync();
            try
            {
                // 3) Double-check cache inside lock
                cached = _cache.Get(serviceName);
                if (cached != null && cached.IsValid())
                    return cached;

                // 4) Validate WSAA Function config
                ValidateWsaaFunctionConfigOrThrow();

                // 5) Call WSAA Function (it returns token/sign/expiration as JSON)
                var ticket = await CallWsaaFunctionAsync(serviceName);

                // 6) Cache + return
                _cache.Set(serviceName, ticket);
                return ticket;

            }
            finally
            {
            known: sem.Release();
            }
        }

        private void ValidateWsaaFunctionConfigOrThrow()
        {
            if (_options.WsaaFunction == null)
                throw new InvalidOperationException("Arca:WsaaFunction section missing.");

            if (string.IsNullOrWhiteSpace(_options.WsaaFunction.BaseUrl))
                throw new InvalidOperationException("Arca:WsaaFunction:BaseUrl missing.");

            if (string.IsNullOrWhiteSpace(_options.WsaaFunction.FunctionCode))
                throw new InvalidOperationException("Arca:WsaaFunction:FunctionCode missing.");

            if (string.IsNullOrWhiteSpace(_options.WsaaFunction.InternalAccessKey))
                throw new InvalidOperationException("Arca:WsaaFunction:InternalAccessKey missing.");
        }

        private async Task<ArcaAccessTicket> CallWsaaFunctionAsync(string serviceName)
        {
            serviceName = serviceName.Trim().ToLowerInvariant();

            var url = $"/api/wsaa/access-ticket?code={Uri.EscapeDataString(_options.WsaaFunction.FunctionCode)}";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.TryAddWithoutValidation("x-internal-key", _options.WsaaFunction.InternalAccessKey);

            req.Content = System.Net.Http.Json.JsonContent.Create(new { serviceName });

            using var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new ArcaDependencyException($"WSAA Function error {(int)resp.StatusCode}: {body}");

            AccessTicketResponseDto? dto;
            try
            {
                dto = System.Text.Json.JsonSerializer.Deserialize<AccessTicketResponseDto>(
                    body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("WSAA Function returned invalid JSON.", ex);
            }

            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Token) ||
                string.IsNullOrWhiteSpace(dto.Sign) ||
                dto.ExpirationTimeUtc == default)
            {
                throw new InvalidOperationException("WSAA Function returned an invalid access ticket payload.");
            }

            return new ArcaAccessTicket
            {
                Token = dto.Token,
                Sign = dto.Sign,
                ExpirationTimeUtc = dto.ExpirationTimeUtc.ToUniversalTime()
            };
        }
    }
}
