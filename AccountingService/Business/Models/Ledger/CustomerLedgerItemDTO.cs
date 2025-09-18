using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Models.Ledger
{
    public class CustomerLedgerItemDTO
    {
        // "Document" o "Receipt"
        public string MovementType { get; set; } = default!;
        public int Id { get; set; }
        public DateTime Date { get; set; }

        // Para documentos
        public LedgerDocumentKind? Kind { get; set; }         // Invoice/DebitNote/CreditNote
        public LedgerDocumentStatus? Status { get; set; }     // Active/Voided
        public decimal? PendingBase { get; set; }             // solo documentos

        // Para recibos
        public decimal? AppliedBase { get; set; }             // suma imputada
        public decimal? UnappliedBase { get; set; }           // total - aplicada

        // Comunes (datos económicos)
        public decimal TotalBase { get; set; }
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; }
        public decimal TotalOriginal { get; set; }

        // Opcional: referencia fiscal (si viene de documento)
        public string? FiscalNumber { get; set; }
    }
}
