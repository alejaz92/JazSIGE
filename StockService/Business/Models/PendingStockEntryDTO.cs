namespace StockService.Business.Models
{
    public class PendingStockEntryDTO
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsProcessed { get; set; }
    }
}
