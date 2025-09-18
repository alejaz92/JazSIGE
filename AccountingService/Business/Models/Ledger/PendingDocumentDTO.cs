using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Models.Ledger
{
    public class PendingDocumentDTO
    {
        public int DocumentId { get; set; }
        public LedgerDocumentKind Kind { get; set; }       // Invoice | DebitNote
        public DateTime Date { get; set; }                 // DocumentDate
        public string? FiscalNumber { get; set; }          // nro fiscal si lo tenés
        public decimal TotalBase { get; set; }             // total en ARS
        public decimal AppliedBase { get; set; }           // Σ imputaciones
        public decimal PendingBase { get; set; }           // TotalBase - AppliedBase
    }
}
