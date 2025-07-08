namespace StockService.Business.Models.StockTransfer
{
    public class StockTransferArticleDTO
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }
}
