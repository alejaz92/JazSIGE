namespace FiscalDocumentationService.Business.Models
{
    public class FiscalDocumentDTO
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public int InvoiceType { get; set; }
        public int PointOfSale { get; set; }
        public DateTime Date { get; set; }

        public string Cae { get; set; } = string.Empty;
        public DateTime CaeExpiration { get; set; }

        public int BuyerDocumentType { get; set; }
        public long BuyerDocumentNumber { get; set; }

        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal ExemptAmount { get; set; }
        public decimal NonTaxableAmount { get; set; }
        public decimal OtherTaxesAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public int? SalesOrderId { get; set; }

        public List<FiscalDocumentItemDTO> Items { get; set; } = new();
    }
}
