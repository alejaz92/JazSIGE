namespace SalesService.Business.Models.SalesOrder
{
    public class SalesOrder_ArticleDTO
    {
        public int ArticleId { get; set; }
        public string ArticleDescription { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }

        public int? DispatchId { get; set; }
        public string ?DispatchCode { get; set; }
    }
}
