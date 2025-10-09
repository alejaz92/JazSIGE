namespace AccountingService.Infrastructure.Models
{
    /// <summary>
    /// Aplica créditos (solo Recibos con saldo) a una Factura/ND en el flujo de creación de factura.
    /// No crea Receipt ni Payments; deja trazabilidad auditable.
    /// </summary>
    public class AllocationBatch : BaseEntity
    {
        public int TargetDocumentId { get; set; }   // LedgerDocument.Id (Invoice/ND)
        public string? Reason { get; set; }       // ej.: "cover-invoice"
        public ICollection<AllocationItem> Items { get; set; } = new List<AllocationItem>();
    }

    public class AllocationItem : BaseEntity
    {
        public int AllocationBatchId { get; set; }
        public AllocationBatch AllocationBatch { get; set; } = default!;

        public int SourceDocumentId { get; set; }       // LedgerDocument.Id (Receipt con saldo)
        public decimal AppliedARS { get; set; }
    }
}
