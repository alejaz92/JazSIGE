namespace PurchaseService.Business.Models.Clients
{
    public class AccountingExternalUpsertDTO
    {
        
        public string PartyType { get; set; } = "supplier";
        public int PartyId { get; set; }

        // LedgerDocumentKind: 0=Invoice, 1=DebitNote, 2=CreditNote
        public string Kind { get; set; }

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
        public string? PartyType { get; set; } = "supplier";
        public string? Kind { get; set; } = "invoice"; // LedgerDocumentKind
        public int InvoiceExternalRefId { get; set; } // Id de la factura en Sales/Fiscal
        public List<CoverInvoiceItem> Items { get; set; } = new();
        public string? Reason { get; set; } = "cover-invoice";
    }

    public class CoverInvoiceItem
    {
        public int SourceLedgerDocumentId { get; set; } // Recibo con saldo (LedgerDocument.Id)
        public decimal AppliedARS { get; set; }
    }

    public class AllocationAdviceDTO
    {
        public bool CanCoverWithReceipts { get; init; }
        public int InvoiceExternalRefId { get; init; }
        public decimal InvoicePendingARS { get; init; }
        public List<ReceiptCreditDTO> Candidates { get; init; } = new();
    }
}
