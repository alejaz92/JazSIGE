namespace AccountingService.Infrastructure.Models.Ledger
{
    /// Documento que impacta la cuenta corriente (sub-ledger).
    /// V1: PartyType = Customer. Supplier preparado para futuro.
    public class LedgerDocument
    {
        public int Id { get; set; }

        public PartyType PartyType { get; set; } = PartyType.Customer; // V1: Customer
        public int PartyId { get; set; }                               // V1: CustomerId

        public LedgerDocumentKind Kind { get; set; }
        public LedgerDocumentStatus Status { get; set; } = LedgerDocumentStatus.Active;

        // === NUEVO: referencia genérica al documento de origen (fiscal o local) ===
        public SourceKind? SourceKind { get; set; }           // FiscalInvoice, ..., AccountingReceipt
        public long? SourceDocumentId { get; set; }           // Id en el sistema de origen (nullable)
        public string? DisplayNumber { get; set; }            // Ej: "A 0001-00012345" o nro de recibo (nullable)

        // === Enlace local cuando el origen es un Recibo ===
        public int? ReceiptId { get; set; }                   // FK opcional a Receipts.Id
        public Receipt? Receipt { get; set; }

        public DateTime DocumentDate { get; set; }

        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; } = 1m;

        public decimal TotalOriginal { get; set; } // enCurrency
        public decimal TotalBase { get; set; }     // enARS

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VoidedAt { get; set; }

        //// ======== COMPAT: campos viejos (para transición) ========
        //// Se mantienen sólo para no romper migración/código temporalmente.
        //// Marcar como [Obsolete] en la próxima iteración y luego eliminar.
        //[System.Obsolete("Use SourceKind/SourceDocumentId/DisplayNumber")]
        //public int FiscalDocumentId { get; set; }

        //[System.Obsolete("Use DisplayNumber")]
        //public string FiscalDocumentNumber { get; set; } = null!;
    }
}
