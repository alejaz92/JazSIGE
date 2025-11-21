namespace PurchaseService.Business.Models
{
    public class PendingStockEntryCreateDTO
    {
        public int PurchaseId { get; set; }
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }

}
