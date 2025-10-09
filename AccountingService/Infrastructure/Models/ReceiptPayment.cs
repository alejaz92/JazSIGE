using static AccountingService.Infrastructure.Models.Enums;

namespace AccountingService.Infrastructure.Models
{
    public class ReceiptPayment : BaseEntity
    {  
        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; } = default!;

        public PaymentMethod Method { get; set; }

        // Multimoneda en pagos
        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; }
        public decimal AmountOriginal { get; set; }
        public decimal AmountARS { get; set; }

        // Lookups externos (CatalogService)
        public int? BankAccountId { get; set; }         // nullable según método
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
        public DateTime? ValueDate { get; set; }

        // Campos para cheques (si aplica)
        public string? CheckBankCode { get; set; }
        public string? CheckNumber { get; set; }
        public string? CheckIssuer { get; set; }
        public bool? IsThirdPartyCheck { get; set; }
        public DateTime? CheckDueDate { get; set; }
    }
}
