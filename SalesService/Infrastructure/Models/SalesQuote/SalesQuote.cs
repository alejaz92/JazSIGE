namespace SalesService.Infrastructure.Models.SalesQuote
{
    public class SalesQuote
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int CustomerId { get; set; }
        public int SellerId { get; set; }
        public int TransportId { get; set; }
        public int PriceListId { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal SubtotalUSD { get; set; }
        public decimal IVAAmountUSD { get; set; }
        public decimal TotalUSD { get; set; }
        public string? Observations { get; set; }

        public ICollection<SalesQuote_Article> Articles { get; set; } = new List<SalesQuote_Article>();
    }
}
