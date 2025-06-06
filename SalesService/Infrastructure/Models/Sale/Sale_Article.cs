namespace SalesService.Infrastructure.Models.Sale
{
    public class Sale_Article
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal IVAPercent { get; set; }

        public Sale Sale { get; set; }
    }
}
