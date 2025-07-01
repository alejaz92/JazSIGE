namespace StockService.Business.Models.CommitedStock
{
    public class CommitedStockSummaryByArticleDTO
    {
        public decimal Total { get; set; }
        public List<CommitedStockSummaryByArticleCustomerDTO> Customers { get; set; } = new List<CommitedStockSummaryByArticleCustomerDTO>();
    }

    public class CommitedStockSummaryByArticleCustomerDTO
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal Quantity { get; set; }
    }
}
