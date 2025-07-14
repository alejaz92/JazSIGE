namespace FiscalDocumentationService.Infrastructure.Models
{
    public class FiscalDocument
    {
        public int Id { get; set; }

        // Enum: Invoice, CreditNote, DebitNote, etc.
        public string DocumentType { get; set; } = null!;

        public string DocumentLetter { get; set; } = "B"; // A, B, C, etc.
        public string PointOfSale { get; set; } = "0001"; // o como string "0001"
        public string DocumentNumber { get; set; } = null!; // Ej: 0001-00000045

        public string CAE { get; set; } = null!;
        public DateTime CAEExpiration { get; set; }

        public DateTime Date { get; set; }

        // Relación con venta
        public int SaleId { get; set; }

        // Datos de la empresa emisora
        public string CompanyName { get; set; } = null!;
        public string CompanyTaxId { get; set; } = null!;
        public string CompanyAddress { get; set; } = null!;

        // Datos del cliente receptor
        public string CustomerName { get; set; } = null!;
        public string CustomerTaxId { get; set; } = null!;
        public string CustomerAddress { get; set; } = null!;
        public string CustomerIVAType { get; set; } = null!;
        public string CustomerSellCondition { get; set; } = null!;

        public decimal Subtotal { get; set; }
        public decimal IVATotal { get; set; }
        public decimal Total { get; set; }

        public ICollection<FiscalDocumentArticle> Articles { get; set; } = new List<FiscalDocumentArticle>();
    }
}
