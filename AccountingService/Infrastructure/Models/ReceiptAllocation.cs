namespace AccountingService.Infrastructure.Models
{
    /// <summary>
    /// Monto aplicado desde el Recibo a un documento target (Invoice/ND/NC/Receipt con saldo).
    /// </summary>
    public class ReceiptAllocation : BaseEntity
    {
        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; } = default!;

        public int TargetDocumentId { get; set; }   // FK (lógica) a LedgerDocument.Id
        public decimal AppliedARS { get; set; }
    }
}
