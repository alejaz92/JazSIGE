namespace SalesService.Business.Models.Clients
{
    /// Request para /api/Documents/fiscal en el microservicio de Accounting.
    /// Los enums viajan como int para evitar dependencias cruzadas.
    public class AccountingFiscalIngestDTO
    {
        // 0 = Customer (V1)
        public int PartyType { get; set; } = 0;
        public int PartyId { get; set; }

        // LedgerDocumentKind: 0=Invoice, 1=DebitNote, 2=CreditNote
        public int Kind { get; set; }

        // SourceKind: 0=FiscalInvoice, 1=FiscalDebitNote, 2=FiscalCreditNote
        public int SourceKind { get; set; }

        // Id del documento en el servicio Fiscal
        public long SourceDocumentId { get; set; }

        public DateTime DocumentDate { get; set; }
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; } = 1m;
        public decimal TotalOriginal { get; set; }

        // Número legible del documento fiscal (ej: "A 0001-00012345")
        public string? DisplayNumber { get; set; }
    }
}
