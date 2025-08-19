namespace StockService.Business.Models.CommitedStock
{
    public class RegisterCommitedStockInputDTO
    {
        public int SaleId { get; set; }
        public int WarehouseId { get; set; }
        public string Reference { get; set; } = string.Empty;

        public List<RegisterCommitedStockArticleInputDTO> Articles { get; set; } = new List<RegisterCommitedStockArticleInputDTO>();

        public bool IsQuick { get; set; } = false;
    }

    public class RegisterCommitedStockArticleInputDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; } = 0;

    }
}
