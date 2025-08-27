using Microsoft.Extensions.Caching.Memory;
using SalesService.Business.Interfaces;
using SalesService.Business.Models.Rates;

namespace SalesService.Business.Services
{
    public class RatesService : IRatesService
    {
        private const string CacheKey = "usdars_oficial_venta";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;


        public RatesService(IHttpClientFactory httpFactory, IMemoryCache cache)
        {
            _http = httpFactory.CreateClient(nameof(RatesService));
            _cache = cache;
        }

        public async Task<ExchangeRateDTO> GetUsdArsOficialAsync()
        {
            if (_cache.TryGetValue(CacheKey, out ExchangeRateDTO? cached) && cached != null)
                return cached;

            // DolarApi (gratuito, sin token): https://dolarapi.com/v1/dolares/oficial
            var resp = await _http.GetFromJsonAsync<DolarApiResp>("https://dolarapi.com/v1/dolares/oficial");
            if (resp == null || resp.venta <= 0)
                throw new InvalidOperationException("No se pudo obtener la cotización oficial (venta).");

            var dto = new ExchangeRateDTO
            {
                Rate = Math.Round(resp.venta, 2),
                Source = "dolarapi",
                FetchedAt = DateTime.UtcNow,
                TtlSeconds = (int)CacheTtl.TotalSeconds
            };

            _cache.Set(CacheKey, dto, CacheTtl);
            return dto;
        }

        private sealed record DolarApiResp(decimal compra, decimal venta, string nombre, string moneda, DateTime fechaActualizacion);
    }
}
