namespace CatalogService.Business.Models.BankAccount
{
    public class BankAccountCreateDTO
    {
        public string AccountNumber { get; set; }
        public int BankId { get; set; }
        public string Cbu { get; set; } = null!;
        public string Alias { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public string AccountType { get; set; } = null!;

    }
}
