using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Models.Ledger
{
    /// Pending debit (Invoice or Debit Note) with remaining balance.
    public class PendingItemDTO
    {
        public int DocumentId { get; set; }
        public LedgerDocumentKind Kind { get; set; }            // Invoice | DebitNote
        public DateTime Date { get; set; }
        public string? Number { get; set; }                     // DisplayNumber
        public decimal TotalBase { get; set; }                  // ARS
        public decimal Applied { get; set; }                    // Σ allocations to this debit
        public decimal Pending { get; set; }                    // TotalBase - Applied
    }
}
