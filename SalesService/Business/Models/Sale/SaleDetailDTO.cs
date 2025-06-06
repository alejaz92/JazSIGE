namespace SalesService.Business.Models.Sale
{
    public class SaleDetailDTO
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public DateTime Date {  get; set; }
        public decimal ExchangeRate { get; set; }
        public string? Observations { get; set; }

        public List<SaleArticleDetailDTO> Articles { get; set; } = new();
    }

    public class SaleArticleDetailDTO
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal IVAPercent { get; set; }
        public decimal Subtotal => Math.Round((UnitPrice * Quantity) * (1 - DiscountPercent / 100), 2);
        public decimal IVAAmount => Math.Round(Subtotal * IVAPercent / 100, 2);
    }
}
