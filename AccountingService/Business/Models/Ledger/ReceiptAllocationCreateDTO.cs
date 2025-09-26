namespace AccountingService.Business.Models.Ledger
{
    /// Asignación de un recibo existente contra un documento débito existente.
    public class ReceiptAllocationCreateDTO
    {
        public int ReceiptId { get; set; }
        public int DebitDocumentId { get; set; }   // Invoice o DebitNote
        public decimal AmountBase { get; set; }    // en ARS
    }
}
