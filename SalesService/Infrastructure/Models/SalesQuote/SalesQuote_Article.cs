namespace SalesService.Infrastructure.Models.SalesQuote
{
    public class SalesQuote_Article
    {
        public int Id { get; set; }
        public int SalesQuoteId { get; set; }
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPriceUSD { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal IVA { get; set; }
        public decimal TotalUSD { get; set; }

        public SalesQuote SalesQuote { get; set; }

    }
}
