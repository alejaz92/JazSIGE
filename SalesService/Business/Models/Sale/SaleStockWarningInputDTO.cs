namespace SalesService.Business.Models.Sale
{
    public class SaleStockWarningInputDTO
    {
        public int SaleId { get; set; }

        // Article that is in conflict due to pending stock changes.
        public int ArticleId { get; set; }

        // Shortage amount at the time the warning was detected.
        public decimal ShortageSnapshot { get; set; }
    }
}
