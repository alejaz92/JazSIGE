namespace CatalogService.Infrastructure.Models
{
    public class BankAccount : BaseEntity
    {
        public string AccountNumber { get; set; } = null!;
        public int BankId { get; set; }
        public Bank Bank { get; set; } = null!;
        public string Cbu { get; set; } = null!;
        public string Alias { get; set; } = null!;
        public string Currency { get; set; } = null!;     
        public string AccountType { get; set; } = null!;

    }
}
