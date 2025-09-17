namespace SalesService.Business.Models.Clients
{
    public class AccountingDocumentCreateDTO
    {
        // V1: siempre clientes
        public string PartyType { get; set; } = "customer";        // 0 = Customer
        public int PartyId { get; set; }                // CustomerId

        public string Kind { get; set; }                  // 0=Invoice, 1=DebitNote, 2=CreditNote

        public int FiscalDocumentId { get; set; }
        public string FiscalDocumentNumber { get; set; } = string.Empty;

        public DateTime DocumentDate { get; set; }
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; } = 1m;
        public decimal TotalOriginal { get; set; }
    }
}
