using AccountingService.Infrastructure.Models.Ledger;

namespace AccountingService.Business.Models.Ledger
{
    public class ReceiptResponseDTO
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public DateTime Date { get; set; }
        public PartyType PartyType { get; set; }
        public int PartyId { get; set; }
        public string Currency { get; set; } = default!;
        public decimal FxRate { get; set; }
        public decimal TotalOriginal { get; set; }
        public decimal TotalBase { get; set; }
        public string? Notes { get; set; }
        public bool IsVoided { get; set; }
        public DateTime? VoidedAt { get; set; }
        public List<PaymentLineResponse> Payments { get; set; } = new();
    }
    public class PaymentLineResponse
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
}
