namespace AccountingService.Business.Models.Ledger
{
    public class CreditItemsAndApplyDTO
    {
        // Para el modal post-factura
        public sealed class CreditItemDTO
        {
            // "receipt" | "creditNote"
            public string Kind { get; init; } = default!;
            public int Id { get; init; }
            public DateTime Date { get; init; }
            public string? Number { get; init; }
            public decimal AvailableBase { get; init; }
        }

        // Aplicación de créditos a UNA factura
        public sealed class CreditPick
        {
            public string Kind { get; init; } = default!; // "receipt" | "creditNote"
            public int Id { get; init; }
        }

        public sealed class ApplyCreditsRequest
        {
            public int InvoiceId { get; init; }
            public int CustomerId { get; init; }
            public IReadOnlyList<CreditPick> Items { get; init; } = Array.Empty<CreditPick>();
        }

        public sealed class AppliedSplit
        {
            public string Kind { get; init; } = default!;
            public int Id { get; init; }
            public decimal AmountBase { get; init; }
        }

        public sealed class ApplyCreditsResult
        {
            public int InvoiceId { get; init; }
            public decimal AppliedTotalBase { get; init; }
            public IReadOnlyList<AppliedSplit> Splits { get; init; } = Array.Empty<AppliedSplit>();
        }
    }
}
