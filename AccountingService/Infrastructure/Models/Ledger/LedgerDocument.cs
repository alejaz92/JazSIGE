namespace AccountingService.Infrastructure.Models.Ledger
{
    /// Documento que impacta la cuenta corriente (sub-ledger).
    /// V1: PartyType = Customer. Supplier queda preparado para futuro.
    public class LedgerDocument
    {
        public int Id { get; set; }

        public PartyType PartyType { get; set; } = PartyType.Customer; // V1: Customer
        public int PartyId { get; set; }                               // V1: CustomerId

        public LedgerDocumentKind Kind { get; set; } 
        public LedgerDocumentStatus Status { get; set; } = LedgerDocumentStatus.Active;

        // Referencia al doc en el servicio origen (Fiscal)
        public int FiscalDocumentId { get; set; }
        public string FiscalDocumentNumber { get; set; } = null!;

        public DateTime DocumentDate { get; set; }

        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; } = 1m;

        public decimal TotalOriginal { get; set; } // enCurrency
        public decimal TotalBase { get; set; }     // enARS

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VoidedAt { get; set; }


    }
}
