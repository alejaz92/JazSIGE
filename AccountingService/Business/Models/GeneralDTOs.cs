namespace AccountingService.Business.Models
{
    using static AccountingService.Infrastructure.Models.Enums;

    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<T> Items { get; set; } = new();
    }

    public class LedgerDocumentDTO
    {
        public int Id { get; set; }
        public PartyType PartyType { get; set; }
        public int PartyId { get; set; }
        public LedgerDocumentKind Kind { get; set; }
        public int ExternalRefId { get; set; }
        public string ExternalRefNumber { get; set; }
        public DateTime DocumentDate { get; set; }
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; }
        public decimal AmountOriginal { get; set; }
        public decimal AmountARS { get; set; }
        public decimal PendingARS { get; set; }
        public DocumentStatus Status { get; set; }
    }

    public class SimpleDocDTO
    {
        public int Id { get; set; }               // LedgerDocument.Id
        public LedgerDocumentKind Kind { get; set; }
        public int ExternalRefId { get; set; }
        public string? ExternalRefNumber { get; set; } 
        public DateTime DocumentDate { get; set; }
        public decimal PendingARS { get; set; }
    }

    public class BalancesDTO
    {
        public decimal PendingToPayARS { get; set; }   // Σ (Invoice+ND) - Σ (NC)
        public decimal CreditInReceiptsARS { get; set; } // Σ (Receipts with Pending)
        public decimal TotalBalanceARS =>   PendingToPayARS - CreditInReceiptsARS;
    }

    public class SelectablesDTO
    {
        public List<SimpleDocDTO> Debits { get; set; } = new();          // Invoice + ND
        public List<SimpleDocDTO> Credits { get; set; } = new();         // NC
        public List<SimpleDocDTO> ReceiptCredits { get; set; } = new();  // Recibos con saldo
    }

    
}
