namespace CatalogService.Infrastructure.Models
{
    public class GrossIncomeType : BaseEntity
    {
        public string Description { get; set; }

        // relations
        public ICollection<Article> Articles { get; set; }
    }
}
