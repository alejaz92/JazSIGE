using System;
using System.Collections.Generic;
using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Models.Ledger
{
    public class ReceiptDTO
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public DateTime Date { get; set; }

        public PartyType PartyType { get; set; }
        public int PartyId { get; set; }

        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; }

        public decimal TotalOriginal { get; set; }
        public decimal TotalBase { get; set; }

        public string? Notes { get; set; }
        public bool IsVoided { get; set; }
        public DateTime? VoidedAt { get; set; }

        public List<PaymentLineDTO> Payments { get; set; } = new();

        /// Convenience: allocations currently linked to this receipt (optional to fill).
        public List<ReceiptAllocationDTO>? Allocations { get; set; }
    }

    public class PaymentLineDTO
    {
        public int Id { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal AmountOriginal { get; set; }
        public decimal AmountBase { get; set; }
        public int? BankAccountId { get; set; }
        public string? TransactionReference { get; set; }
        public string? Notes { get; set; }
        public DateTime? ValueDate { get; set; }
    }

    public class ReceiptAllocationDTO
    {
        public int Id { get; set; }
        public int DebitDocumentId { get; set; }
        public decimal AmountBase { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
