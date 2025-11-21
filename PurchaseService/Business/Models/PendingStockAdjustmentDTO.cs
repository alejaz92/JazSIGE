namespace PurchaseService.Business.Models
{
    public class PendingStockAdjustmentItemDTO
    {
        public int ArticleId { get; set; }
        public decimal OldQuantity { get; set; }
        public decimal NewQuantity { get; set; }
        public decimal? NewUnitCost { get; set; }
    }

    public class StockConflictSaleRefDTO
    {
        public int SalesOrderId { get; set; }
        public decimal RemainingCommitted { get; set; }
    }

    public class StockConflictPerArticleDTO
    {
        public int ArticleId { get; set; }
        public decimal AvailableBefore { get; set; }
        public decimal AvailableAfter { get; set; }
        public decimal Shortage { get; set; }
        public List<StockConflictSaleRefDTO> ImplicatedSales { get; set; } = new();
    }

    public class StockApplyAdjustmentResultDTO
    {
        public int PurchaseId { get; set; }
        public List<StockConflictPerArticleDTO> Conflicts { get; set; } = new();
    }

}
