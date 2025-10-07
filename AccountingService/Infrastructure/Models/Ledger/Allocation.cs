namespace AccountingService.Infrastructure.Models.Ledger
{
    public enum AllocationSource : byte { Receipt = 1, CreditDocument = 2 }

    /// Imputación: cuánto baja una Factura/ND desde un Recibo **o** una Nota de Crédito.
    public class Allocation
    {
        public int Id { get; set; }

        public AllocationSource Source { get; set; }

        public int? ReceiptId { get; set; }
        public Receipt? Receipt { get; set; } = default!;

        public int? CreditDocumentId { get; set; }   // LedgerDocument (Kind = CreditNote)
        public LedgerDocument? CreditDocument { get; set; } = default!;

        public int DebitDocumentId { get; set; } // LedgerDocument (Kind = Invoice/DebitNote)
        public LedgerDocument DebitDocument { get; set; } = default!;

        public decimal AmountBase { get; set; } // siempre en ARS
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
