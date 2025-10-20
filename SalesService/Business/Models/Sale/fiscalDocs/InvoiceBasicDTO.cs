using SalesService.Business.Models.Sale.accounting;

namespace SalesService.Business.Models.Sale.fiscalDocs
{
    public class InvoiceBasicDTO
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public int InvoiceType { get; set; }
        public int PointOfSale { get; set; }
        public DateTime Date { get; set; }

        public string Cae { get; set; } = string.Empty;
        public DateTime CaeExpiration { get; set; }

        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public AllocationAdviceDTO? AllocationAdvice { get; set; }
    }
}
