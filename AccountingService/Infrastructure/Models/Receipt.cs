using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AccountingService.Infrastructure.Models
{
    /// <summary>
    /// Recibo (propio de Accounting). Se refleja en LedgerDocument con Kind=Receipt
    /// y ExternalRefId = Receipt.Id
    /// </summary>
    public class Receipt : BaseEntity
    {
        public string? Number { get; set; } // Número del recibo, p.ej. "R0001-00001234"
        public string? Notes { get; set; }

        public ICollection<ReceiptPayment> Payments { get; set; } = new List<ReceiptPayment>();
        public ICollection<ReceiptAllocation> Allocations { get; set; } = new List<ReceiptAllocation>();
    }
}
