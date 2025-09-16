namespace AccountingService.Infrastructure.Models.Ledger
{
    public enum PartyType : byte { Customer = 0, Supplier = 1 }
    public enum LedgerDocumentKind : byte { Invoice = 0, DebitNote = 1, CreditNote = 2}
    public enum LedgerDocumentStatus : byte { Active = 0, Voided = 1 }
    public enum PaymentMethod : byte 
    { 
        Cash = 0,
        BankTransfer = 1,
        CreditCard = 2,
        DebitCard = 3,
        Check = 4,
        BankDeposit = 5,
        Other = 99
    }
}
