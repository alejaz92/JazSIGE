namespace SalesService.Business.Models.SalesQuote
{
    public class SalesQuoteArticleCreateDTO
    {
        public int ArticleId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPriceUSD { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal IVA { get; set; }
    }
}
