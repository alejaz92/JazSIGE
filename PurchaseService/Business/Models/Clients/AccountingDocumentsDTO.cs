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
}
