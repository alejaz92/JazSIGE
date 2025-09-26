namespace AccountingService.Infrastructure.Models.Ledger
{
    public enum PartyType : byte { Customer = 0, Supplier = 1 }

    // Documento que se lista en el sub-ledger
    public enum LedgerDocumentKind : byte
    {
        Invoice = 0,
        DebitNote = 1,
        CreditNote = 2,
        Receipt = 3
    }

    public enum LedgerDocumentStatus : byte { Active = 0, Voided = 1 }

    // NUEVO: Origen del documento en su sistema fuente
    public enum SourceKind : byte
    {
        // Fiscales (otro microservicio)
        FiscalInvoice = 0,
        FiscalDebitNote = 1,
        FiscalCreditNote = 2,

        // Contables locales
        AccountingReceipt = 10
    }

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
