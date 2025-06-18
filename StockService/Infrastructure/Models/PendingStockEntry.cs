namespace StockService.Infrastructure.Models
{
    public class PendingStockEntry
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsProcessed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
