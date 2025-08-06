namespace StockService.Infrastructure.Models
{
    public class CommitedStockEntry
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ArticleId { get; set; }
        public bool IsFinalConsumer { get; set; }
        public int? CustomerId { get; set; } 
        public string CustomerName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Delivered { get; set; }
        public decimal Remaining => Quantity - Delivered;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
