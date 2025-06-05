namespace SalesService.Infrastructure.Models.SalesOrder
{
    public class SalesOrder_Article
    {
        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
        public int? DispatchId { get; set; }

        public SalesOrder SalesOrder { get; set; } = null!;
    }
}
