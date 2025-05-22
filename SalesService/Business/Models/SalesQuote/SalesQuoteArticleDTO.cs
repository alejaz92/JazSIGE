public class SalesQuoteArticleDTO
{
    public int ArticleId { get; set; }
    public string ArticleName { get; set; } = string.Empty;
    public string ArticleSKU { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public decimal UnitPriceUSD { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal IVA { get; set; }
    public decimal TotalUSD { get; set; }
}
