namespace AccountingService.Business.Models.Clients
{
    public class CompanyInfoDTO
    {
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string IVAType { get; set; } = string.Empty;
        public string GrossIncome { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string LogoUrl { get; set; } = string.Empty;
        public DateTime DateOfIncorporation { get; set; }
    }
}
