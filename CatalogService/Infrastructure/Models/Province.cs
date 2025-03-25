namespace CatalogService.Infrastructure.Models
{
    public class Province : BaseEntity
    {
        public string Name { get; set; }
        public int CountryId { get; set; }
        public Country Country { get; set; }

        //relations
        public IEnumerable<City> Cities { get; set; }
    }
}
