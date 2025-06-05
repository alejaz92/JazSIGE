namespace StockService.Infrastructure.Models
{
    public class StockByDispatch
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int? DispatchId { get; set; }
        public decimal Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    }
}
