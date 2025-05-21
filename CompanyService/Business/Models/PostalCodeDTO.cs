namespace CompanyService.Business.Models
{
    public class PostalCodeDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public CityDTO City { get; set; }
    }

    public class CityDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public ProvinceDTO Province { get; set; }
    }

    public class ProvinceDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public CountryDTO Country { get; set; }
    }

    public class CountryDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }
}
