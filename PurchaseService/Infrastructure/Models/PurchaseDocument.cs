namespace PurchaseService.Infrastructure.Models
{
    public class PurchaseDocument
    {
        public int Id { get; set; }

        //FK
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; } = null!;

        // core data
        public PurchaseDocumentType Type { get; set; }
        public string Number { get; set; } = null!;
        public DateTime Date { get; set; }

        // Currency info (USD or ARS per current scope)
        public string Currency { get; set; } = null!;
        public decimal FxRate { get; set; }
        public decimal TotalAmount { get; set; }

        // Link to the scanned/digital document
        public string FileUrl { get; set; } = null!;

        // Audit & life cycle
        public bool IsCanceled { get; set; } = false;
        public DateTime? CanceledAt { get; set; }
        public string? CanceledReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
