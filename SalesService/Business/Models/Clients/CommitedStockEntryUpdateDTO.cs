namespace SalesService.Business.Models.Clients
{
    public class CommitedStockEntryUpdateDTO
    {
        public int SaleId { get; set; }
        public List<CommitedStockEntryArticleUpdateDTO> Articles { get; set; } = new();
        
    }

    public class CommitedStockEntryArticleUpdateDTO
    {
        public int ArticleId { get; set; }
        public decimal NewQuantity { get; set; }
    }
}
