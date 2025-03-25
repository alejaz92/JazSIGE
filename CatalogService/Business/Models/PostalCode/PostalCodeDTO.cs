namespace CatalogService.Business.Models.PostalCode
{
    public class PostalCodeDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
    }
}
