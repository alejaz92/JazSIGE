namespace StockService.Business.Models.CommitedStock
{
    public class CommitedStockEntryCreateDTO
    {
        public int SaleId { get; set; }
        public bool IsFinalConsumer { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
    }
}
