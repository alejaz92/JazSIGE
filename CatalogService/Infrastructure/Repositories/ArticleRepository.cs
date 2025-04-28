using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
    public class ArticleRepository : GenericRepository<Article>, IArticleRepository
    {
        private readonly CatalogDbContext _dbContext;

        public ArticleRepository(CatalogDbContext dbContext) : base(dbContext) 
        {
            _dbContext = dbContext;
        }
    }
}
