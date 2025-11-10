using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Business.Models
{
    public class PurchaseDocumentDTO
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }

        public PurchaseDocumentType Type { get; set; }
        public string Number { get; set; } = null!;
        public DateTime Date { get; set; }

        public string Currency { get; set; } = null!;
        public decimal FxRate { get; set; }
        public decimal TotalAmount { get; set; }

        public string FileUrl { get; set; } = null!;

        public bool IsCanceled { get; set; }
        public DateTime? CanceledAt { get; set; }
        public string? CanceledReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
