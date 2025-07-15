namespace FiscalDocumentationService.Business.Models
{
    public class FiscalDocumentCreateDTO
    {
        public int SaleId { get; set; }
        public string DocumentType { get; set; } = "Invoice"; // Podría ser "Invoice", "CreditNote", etc.
        public string DocumentLetter { get; set; } = "B";     // A, B, C
        public string PointOfSale { get; set; } = "0001";
    }
}
