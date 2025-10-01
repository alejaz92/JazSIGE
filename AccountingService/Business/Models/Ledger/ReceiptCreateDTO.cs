using AccountingService.Infrastructure.Models.Ledger;
using System.ComponentModel.DataAnnotations;

namespace AccountingService.Business.Models.Ledger
{
    /// Receipt command: creates Receipt + its mirror LedgerDocument + optional allocations.
    public class ReceiptCreateDTO
    {
        [Required] public PartyType PartyType { get; set; } = PartyType.Customer;
        [Required] public int PartyId { get; set; }

        [Required] public DateTime Date { get; set; }

        [Required, StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "ARS";

        [Range(0.000001, 999999)]
        public decimal FxRate { get; set; } = 1m;

        [StringLength(500)]
        public string? Notes { get; set; }

        [MinLength(1)]
        public List<PaymentLineCreateDTO> Payments { get; set; } = new();

        /// Optional initial allocations done within the same transaction.
        public List<AllocationCreateDTO>? Allocations { get; set; }
    }

    public class PaymentLineCreateDTO
    {
        public PaymentMethod Method { get; set; }
        public decimal AmountOriginal { get; set; }

        public int? BankAccountId { get; set; }
        public string? TransactionReference { get; set; }

        // Cheque (usar si Method == Check)
        public string? CheckIssuerBankCode { get; set; }
        public string? CheckNumber { get; set; }
        public DateTime? CheckIssueDate { get; set; }
        public DateTime? CheckPaymentDate { get; set; }
        public string? CheckIssuerTaxId { get; set; }
        public string? CheckIssuerName { get; set; }
        public bool? CheckIsThirdParty { get; set; }


        public string? Notes { get; set; }
        public DateTime? ValueDate { get; set; } // si no se envía y es cheque, se tomará CheckPaymentDate
    }

    public class AllocationCreateDTO
    {
        [Required] public int DebitDocumentId { get; set; }     // target invoice/debit note
        [Range(0.01, 999_999_999)]
        public decimal AmountBase { get; set; }                 // in ARS
    }
}
