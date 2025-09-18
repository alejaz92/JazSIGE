using AccountingService.Infrastructure.Models.Ledger;
using System.ComponentModel.DataAnnotations;

namespace AccountingService.Business.Models.Ledger
{
    public class ReceiptRequestCreateDTO
    {
        [Required] 
        public PartyType PartyType { get; set; } = PartyType.Customer;

        [Required] 
        public int PartyId { get; set; }

        [Required] 
        public DateTime Date { get; set; }

        [Required, StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "ARS";

        [Range(0.000001, 999999)]
        public decimal FxRate { get; set; } = 1m;

        [StringLength(500)]
        public string? Notes { get; set; }

        [MinLength(1)]
        public List<PaymentLineRequest> Payments { get; set; } = new();
        // Nota: TotalOriginal/Base del recibo se computa a partir de Payments.
        
        public List<AllocationItemRequest>? Allocations { get; set; } 
    }

    public class PaymentLineRequest
    {
        [Required] 
        public PaymentMethod Method { get; set; }

        [Range(0.01, 999_999_999)]
        public decimal AmountOriginal { get; set; } // en Currency

        // Si es transferencia/depósito, registrar cuenta y referencia bancaria
        public int? BankAccountId { get; set; }             // id de cuenta de TU empresa
        [StringLength(100)] public string? TransactionReference { get; set; }

        [StringLength(100)] public string? Notes { get; set; }

        public DateTime? ValueDate { get; set; }
        // Nota: AmountBase por línea se calcula = AmountOriginal * FxRate del recibo
    }

    public class AllocationItemRequest
    {
        [Required]
        public int DocumentId { get; set; } // Id de factura/ND a imputar
        [Range(0.01, 999_999_999)]
        public decimal AmountBase { get; set; } // en ARS
    }
}
