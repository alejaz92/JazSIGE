namespace CatalogService.Business.Models.BankAccount
{
    public class BankAccountDTO
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string Cbu { get; set; } = null!;
        public string Alias { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public string AccountType { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
