namespace FiscalDocumentationService.Business.Models
{
    public class FiscalDocumentDTO
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public string CAE { get; set; } = string.Empty;
        public DateTime CAEExpiration { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerCUIT { get; set; } = string.Empty;
        public string CustomerIVAType { get; set; } = string.Empty;

        public decimal NetAmount { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public int? SalesOrderId { get; set; }

        public List<FiscalDocumentItemDTO> Items { get; set; } = new();
    }
}
