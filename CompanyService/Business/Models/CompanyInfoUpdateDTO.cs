namespace CompanyService.Business.Models
{
    public class CompanyInfoUpdateDTO
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string TaxId { get; set; }
        public string Address { get; set; }
        public int PostalCodeId { get; set; }
        public int IVATypeId { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LogoUrl { get; set; }
        public string GrossIncome { get; set; }
        public DateTime DateOfIncorporation { get; set; }
    }
}
