using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Models.Ledger
{
    public class DocumentResponseDTO
    {
        public int Id { get; set; }
        public PartyType PartyType { get; set; }
        public int PartyId { get; set; }
        public LedgerDocumentKind Kind { get; set; }
        public LedgerDocumentStatus Status { get; set; }
        public int FiscalDocumentId { get; set; }
        public DateTime DocumentDate { get; set; }
        public string Currency { get; set; } = default!;
        public decimal FxRate { get; set; }
        public decimal TotalOriginal { get; set; }
        public decimal TotalBase { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VoidedAt { get; set; }
    }
}
