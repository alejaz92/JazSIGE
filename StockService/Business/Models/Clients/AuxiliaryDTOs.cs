namespace StockService.Business.Models.Clients
{
    public class ArticleDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class WarehouseDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UserDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class TransportDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? TaxId { get; set; }
        public string Address { get; set; }
        public int PostalCodeId { get; set; }
        public string PostalCode { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public int ProvinceId { get; set; }
        public string Province { get; set; }
        public int CountryId { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
        public bool IsActive { get; set; }
    }

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
