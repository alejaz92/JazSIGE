namespace SalesService.Infrastructure.Models.SalesQuote
{
    public class SalesQuote
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsFinalConsumer { get; set; } = false;
        public string CustomerIdType { get; set; } = "CUIT"; // Default value
        public int? CustomerId { get; set; }
        public string? CustomerTaxId { get; set; }
        public string? CustomerName { get; set; } = string.Empty;
        public int? CustomerPostalCodeId { get; set; }
        public int SellerId { get; set; }
        public int PriceListId { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal SubtotalUSD { get; set; }
        public decimal IVAAmountUSD { get; set; }
        public decimal TotalUSD { get; set; }
        public string? Observations { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SalesQuote_Article> Articles { get; set; } = new List<SalesQuote_Article>();
    }
}
