namespace PurchaseService.Business.Models
{
    public class DispatchDTO
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int PurchaseId { get; set; }
    }
}
