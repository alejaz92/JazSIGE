using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Models.Ledger
{
    public class LedgerItemDTO
    {
        public int DocumentId { get; set; }
        public LedgerDocumentKind Kind { get; set; }            // Invoice | DebitNote | CreditNote | Receipt
        public LedgerDocumentStatus Status { get; set; }        // Active | Voided
        public long SourceDocumentId { get; set; }  // En recibos, el Id del recibo original

        public DateTime Date { get; set; }                      // DocumentDate
        public string? Number { get; set; }                     // DisplayNumber (invoice/ND/NC or receipt number)

        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; }
        public decimal TotalOriginal { get; set; }
        public decimal TotalBase { get; set; }
    }
}
