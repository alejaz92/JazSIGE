namespace AccountingService.Infrastructure.Models.Ledger
{
    /// Imputación: cuánto de un Recibo baja una Factura/ND.
    public class Allocation
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; } = default!;

        public int DebitDocumentId { get; set; }
        public LedgerDocument DebitDocument { get; set; } = default!;

        public decimal AmountBase { get; set; } // siempre en ARS
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
