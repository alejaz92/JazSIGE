namespace SalesService.Business.Models.Clients
{
    public class CommitedStockEntryCreateDTO
    { 
        public int SaleId { get; set; }
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }   
    }
}
