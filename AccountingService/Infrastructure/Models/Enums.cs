namespace AccountingService.Infrastructure.Models
{
    public class Enums
    {
        public enum PartyType
        {
            Customer = 1,
            Supplier = 2
        }

        public enum LedgerDocumentKind
        {
            Invoice = 1,
            DebitNote = 2,
            CreditNote = 3,
            Receipt = 4
        }

        public enum DocumentStatus
        {
            Active = 1,
            Voided = 2
        }

        public enum PaymentMethod
        {
            Cash = 1,
            Transfer = 2,
            Deposit = 3,
            Check = 4,
            Other = 99
        }
    }
}
