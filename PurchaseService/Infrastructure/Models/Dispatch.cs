namespace PurchaseService.Infrastructure.Models
{
    public class Dispatch
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Origin { get; set; } = null!;
        public DateTime Date { get; set; }

        public int PurchaseId { get; set; } 
        public Purchase Purchase { get; set; }

    }
}
