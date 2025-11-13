namespace SalesService.Infrastructure.Models.Sale
{
    public class SaleStockWarning
    {
        public int Id { get; set; }

        // Parent sale that is affected by this stock warning.
        public int SaleId { get; set; }

        // Article that is in conflict (insufficient available stock).
        public int ArticleId { get; set; }

        // Snapshot of the shortage when the warning was created.
        public decimal ShortageSnapshot { get; set; }

        // Indicates if this warning is still active or has been resolved.
        public bool IsResolved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Timestamp when the warning was resolved (if applicable).
        public DateTime? ResolvedAt { get; set; }

        // Navigation property back to the sale.
        public Sale Sale { get; set; }
    }
}
