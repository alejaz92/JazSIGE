namespace CatalogService.Infrastructure.Models
{
    public class City : BaseEntity
    {
        public string Name { get; set; }
        public int ProvinceId { get; set; }
        public Province Province { get; set; }

        // relations
        public IEnumerable<PostalCode> PostalCodes { get; set; }
    }
}
