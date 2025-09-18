namespace AccountingService.Infrastructure.Models.Ledger
{
    /// Recibo (cobro real). V1: PartyType = Customer.
    public class Receipt
    {
        public int Id { get; set; }
        public string? Number { get; set; } // Numero de recibo (opcional)
        public DateTime Date { get; set; }

        public PartyType PartyType { get; set; } = PartyType.Customer; // V1: Customer
        public int PartyId { get; set; }                               // V1: CustomerId

        public string Currency { get; set; } = "ARS";
        public decimal FxRate { get; set; } = 1m;

        public decimal TotalOriginal { get; set; } // enCurrency
        public decimal TotalBase { get; set; }     // enARS

        public string? Notes { get; set; }

        public bool IsVoided { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VoidedAt { get; set; }

        public ICollection<PaymentLine> PaymentLines { get; set; } = new List<PaymentLine>();
        public ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
    }
}
