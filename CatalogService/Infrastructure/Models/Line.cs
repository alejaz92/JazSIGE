namespace CatalogService.Infrastructure.Models
{
    public class Line : BaseEntity
    {
        public string Description { get; set; }

        public int LineGroupId { get; set; }
        public LineGroup LineGroup { get; set; }

        // relations
        public ICollection<Article> Articles { get; set; }
    }
}
