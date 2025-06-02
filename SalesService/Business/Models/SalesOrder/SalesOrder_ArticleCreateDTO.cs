namespace SalesService.Business.Models.SalesOrder
{
    public class SalesOrder_ArticleCreateDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
    }
}
