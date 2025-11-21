namespace StockService.Business.Models.PendingStock
{
    public class PurchasePendingAdjustmentItemDTO
    {
        public int ArticleId { get; set; }
        public decimal OldQuantity { get; set; }   // previous pending quantity for this purchase/article
        public decimal NewQuantity { get; set; }   // new pending quantity for this purchase/article
        public decimal? NewUnitCost { get; set; }    // new unit cost for this purchase/article
    }

    public class PurchasePendingAdjustmentDTO
    {
        public int PurchaseId { get; set; }
        public List<PurchasePendingAdjustmentItemDTO> Items { get; set; } = new();
        public int UserId { get; set; }           // for auditing if needed
    }
}
