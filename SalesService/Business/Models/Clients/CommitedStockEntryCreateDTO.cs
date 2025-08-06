namespace SalesService.Business.Models.Clients
{
    public class CommitedStockEntryCreateDTO
    { 
        public int SaleId { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }   
    }
}
