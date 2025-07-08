namespace StockService.Infrastructure.Models
{
    public class StockTransfer_Article
    {
        public int Id { get; set; }
        public int StockTransferId { get; set; }
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }

        public StockTransfer? StockTransfer { get; set; }
    }
}
