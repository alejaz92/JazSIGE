namespace CatalogService.Infrastructure.Models
{
    public class PostalCode : BaseEntity
    {
        public string Code { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }

        // relations
        public IEnumerable<Transport> Transports { get; set; }
    }
}
