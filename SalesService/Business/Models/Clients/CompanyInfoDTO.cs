namespace SalesService.Business.Models.Clients
{
    public class CompanyInfoDTO
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string TaxId { get; set; }
        public string Address { get; set; }

        public int PostalCodeId { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LogoUrl { get; set; }

        public int IVATypeId { get; set; }
        public string IVAType { get; set; }

        public string GrossIncome { get; set; }
        public DateTime DateOfIncorporation { get; set; }
    }
}
