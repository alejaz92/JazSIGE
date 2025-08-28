namespace SalesService.Business.Models.Sale.fiscalDocs
{
    public class SaleNoteSummaryDTO
    {
        public int Id { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string Kind { get; set; } = string.Empty;
    }
}
