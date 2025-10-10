namespace SalesService.Business.Models.Clients
{
    /// Request para /api/accounting/external-documents en Accounting.
    /// Enums viajan como int para evitar dependencia de tipos.
    public class AccountingExternalUpsertDTO
    {
        // 0 = Customer
        public int PartyType { get; set; } = 0;
        public int PartyId { get; set; }

        // LedgerDocumentKind: 0=Invoice, 1=DebitNote, 2=CreditNote
        public int Kind { get; set; }

        public long ExternalRefId { get; set; }
        public string ExternalRefNumber { get; set; } = string.Empty;

        public DateTime DocumentDate { get; set; }

        // Docs fiscales vienen en ARS; mantenemos campos por compat
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; } = 1m;

        // Total en ARS usado por Accounting para Pending/Amount
        public decimal AmountARS { get; set; }
    }

    public class ReceiptCreditDTO
    {
        public int Id { get; set; }                 // LedgerDocumentId del recibo con saldo
        public string ExternalRefNumber { get; set; } = string.Empty; // Nro de recibo
        public DateTime DocumentDate { get; set; }
        public decimal PendingARS { get; set; }
    }

    public class CoverInvoiceRequest
    {
        public int PartyId { get; set; }
        public int InvoiceExternalRefId { get; set; } // Id de la factura en Sales/Fiscal
        public List<CoverInvoiceItem> Items { get; set; } = new();
        public string? Reason { get; set; } = "cover-invoice";
    }

    public class CoverInvoiceItem
    {
        public int SourceLedgerDocumentId { get; set; } // Recibo con saldo (LedgerDocument.Id)
        public decimal AppliedARS { get; set; }
    }

}
