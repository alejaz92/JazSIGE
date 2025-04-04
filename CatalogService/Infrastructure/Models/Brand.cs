﻿namespace CatalogService.Infrastructure.Models
{
    public class Brand : BaseEntity
    {
        public string Description { get; set; }

        // relations
        public ICollection<Article> Articles { get; set; }

    }
}
