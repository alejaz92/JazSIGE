namespace AccountingService.Infrastructure.Models.Ledger
{
    /// Línea de pago del recibo (transferencia, depósito, etc.)
    public class PaymentLine
    {
        public int Id { get; set; }

        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; } = default!;

        public PaymentMethod Method { get; set; }

        public decimal AmountOriginal { get; set; } // enCurrency
        public decimal AmountBase { get; set; }     // enARS

        public int? BankAccountId { get; set; } // opcional, si aplica
        public string? TransactionReference { get; set; } // opcional, si aplica    

        public string? Notes { get; set; } // opcional

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ValueDate { get; set; }
    }
}
