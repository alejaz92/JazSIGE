namespace CatalogService.Business.Models.PostalCode
{
    public class PostalCodeDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public int ProvinceId { get; set; }
        public string Province { get; set; }
        public int CountryId { get; set; }
        public string Country { get; set; }

    }
}
