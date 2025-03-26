namespace CatalogService.Infrastructure.Models
{
    public class Unit : BaseEntity
    {
        public string Description { get; set; }

        //relations
        public ICollection<Article> Articles { get; set; }
    }
}
