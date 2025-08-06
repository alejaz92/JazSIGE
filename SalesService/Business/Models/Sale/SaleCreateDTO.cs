namespace SalesService.Business.Models.Sale
{
    public class SaleCreateDTO
    {
        public bool IsFinalConsumer { get; set; }
        public string CustomerIdType { get; set; } = "CUIT"; // Default value
        public string? CustomerTaxId { get; set; }
        public string? CustomerName { get; set; }
        public int? CustomerId { get; set; }
        public int? CustomerPostalCodeId { get; set; }
        public int SellerId { get; set; }
        public DateTime Date { get; set; }
        public decimal ExchangeRate { get; set; }
        public string? Observations { get; set; }
        public List<SaleArticleCreateDTO> Articles { get; set; } // Changed from private to public
    }

    public class SaleArticleCreateDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal IVAPercent { get; set; }
    }
}
