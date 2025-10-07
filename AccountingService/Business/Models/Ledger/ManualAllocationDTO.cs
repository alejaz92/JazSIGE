namespace AccountingService.Business.Models.Ledger
{
    public class ManualAllocationDTO
    {
        public enum ManualAllocationSourceKind {  Receipt = 1, CreditDocument = 2 }

        public sealed record ManualAllocationSourceLink(
            ManualAllocationSourceKind SourceKind,
            int SourceId,
            decimal AmountBase
        );

        // Un débito (Factura/ND) con sus fuentes elegidas por el usuario
        public sealed class ManualAllocationDebitSet
        {
            public int DebitDocumentId { get; set; }
            public List<ManualAllocationSourceLink> Sources { get; set; } = new();
        }

        // Pedido de ejecución por lote (1..N débitos); cada uno debe quedar 100% cubierto
        public sealed class ManualAllocationExecuteDTO
        {
            public int CustomerId { get; set; }
            public List<ManualAllocationDebitSet> Debits { get; set; } = new();
        }

        // Preview y resultado (misma forma para simplificar UI)
        public sealed class ManualAllocationPreviewDTO
        {
            public int CustomerId { get; set; }
            public List<ManualAllocationDebitSet> Debits { get; set; } = new();
            public decimal TotalApplyBase => Debits.Sum(d => d.Sources.Sum(s => s.AmountBase));
            public List<string> Warnings { get; set; } = new(); // motivos de no ejecución
            public bool CanExecute => Warnings.Count == 0;
        }
    }
}
