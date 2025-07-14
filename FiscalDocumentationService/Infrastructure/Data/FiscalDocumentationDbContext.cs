using FiscalDocumentationService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace FiscalDocumentationService.Infrastructure.Data
{
    public class FiscalDocumentationDbContext : DbContext
    {
        public FiscalDocumentationDbContext(DbContextOptions<FiscalDocumentationDbContext> options) : base(options) { }

        public DbSet<FiscalDocument> FiscalDocuments { get; set; }
        public DbSet<FiscalDocumentArticle> FiscalDocumentArticles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
