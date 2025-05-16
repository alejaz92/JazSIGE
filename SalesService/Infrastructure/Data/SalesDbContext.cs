using Microsoft.EntityFrameworkCore;
using SalesService.Infrastructure.Models;

namespace SalesService.Infrastructure.Data
{
    public class SalesDbContext : DbContext
    {
        public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
        {

        }

        public DbSet<ArticlePriceList> ArticlePriceLists { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
