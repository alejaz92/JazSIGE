using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Models.Ledger
{
    public class DocumentDTO
    {
        public int Id { get; set; }

        public PartyType PartyType { get; set; }
        public int PartyId { get; set; }

        public LedgerDocumentKind Kind { get; set; }
        public LedgerDocumentStatus Status { get; set; }

        // Generic source reference (fiscal or local)
        public SourceKind? SourceKind { get; set; }
        public long? SourceDocumentId { get; set; }

        // Present only when the origin is a local Receipt
        public int? ReceiptId { get; set; }

        public DateTime DocumentDate { get; set; }
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; }
        public decimal TotalOriginal { get; set; }
        public decimal TotalBase { get; set; }

        public string? Number { get; set; }                     // DisplayNumber
        public DateTime CreatedAt { get; set; }
        public DateTime? VoidedAt { get; set; }
    }
}
