namespace StockService.Business.Models.CommitedStock
{
    public class CommitedStockEntryDTO
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Delivered { get; set; }
        public decimal Remaining { get; set; }
    }
}
