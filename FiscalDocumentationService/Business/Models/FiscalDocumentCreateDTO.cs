namespace FiscalDocumentationService.Business.Models
{
    public class FiscalDocumentCreateDTO
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerCUIT { get; set; } = string.Empty;
        public string CustomerIVAType { get; set; } = string.Empty;

        public decimal NetAmount { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public int? SalesOrderId { get; set; }

        public string Type { get; set; } = "Invoice"; // "Invoice", "CreditNote", "DebitNote"

        public List<FiscalDocumentItemDTO> Items { get; set; } = new();
    }

    public class FiscalDocumentItemDTO
    {
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal VAT { get; set; }
    }
}
