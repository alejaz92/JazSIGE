namespace CatalogService.Infrastructure.Models
{
    public class Country : BaseEntity
    {
        public string Name { get; set; }

        // relations
        public IEnumerable<Province> Provinces { get; set; }
    }
}
