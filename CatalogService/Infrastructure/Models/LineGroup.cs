namespace CatalogService.Infrastructure.Models
{
    public class LineGroup : BaseEntity
    {
        public string Description { get; set; }

        // relations
        public ICollection<Line> Lines { get; set; }
    }
}
