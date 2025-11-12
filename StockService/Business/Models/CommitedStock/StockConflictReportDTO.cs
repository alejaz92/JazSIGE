namespace StockService.Business.Models.CommitedStock
{
    public class StockConflictSaleRefDTO
    {
        public int SaleId { get; set; }      // id from Sales domain
        public decimal RemainingCommitted { get; set; }  // quantity still to deliver on that sales line/order
    }

    public class StockConflictPerArticleDTO
    {
        public int ArticleId { get; set; }
        public decimal AvailableBefore { get; set; }  // onHand + pendingUnprocessed - committedRemaining
        public decimal AvailableAfter { get; set; }   // AvailableBefore + delta
        public decimal Shortage => AvailableAfter < 0 ? -AvailableAfter : 0;
        public List<StockConflictSaleRefDTO> ImplicatedSales { get; set; } = new(); // ordered FIFO
    }

    public class StockApplyAdjustmentResultDTO
    {
        public int PurchaseId { get; set; }
        public List<StockConflictPerArticleDTO> Conflicts { get; set; } = new();
    }
}
