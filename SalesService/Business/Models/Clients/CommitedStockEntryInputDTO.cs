namespace SalesService.Business.Models.Clients
{
    public class CommitedStockInputDTO
    {
        public int SaleId { get; set; }
        public int WarehouseId { get; set; }
        public string Reference { get; set; } = string.Empty;

        public List<CommitedStockArticleInputDTO> Articles { get; set; } = new List<CommitedStockArticleInputDTO>();

        public bool IsQuick { get; set; } = false;
    }

    public class CommitedStockArticleInputDTO
    {
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; } = 0;
    }
}
