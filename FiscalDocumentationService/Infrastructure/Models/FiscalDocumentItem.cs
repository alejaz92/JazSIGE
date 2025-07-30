namespace FiscalDocumentationService.Infrastructure.Models
{
    public class FiscalDocumentItem
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal VAT { get; set; }

        public int FiscalDocumentId { get; set; }
        public FiscalDocument FiscalDocument { get; set; } = null!;
    }
}
