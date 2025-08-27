namespace SalesService.Business.Models.Rates
{
    public class ExchangeRateDTO
    {
        public decimal Rate { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; }
        public int TtlSeconds { get; set; }
    }
}
