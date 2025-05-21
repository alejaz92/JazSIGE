using CompanyService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CompanyService.Infrastructure.Data
{
    public class CompanyDbContext : DbContext
    {
        public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options)
        {
        }

        public DbSet<CompanyInfo> CompanyInfo { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyInfo>().HasKey(c => c.Id);
            base.OnModelCreating(modelBuilder);
        }
    }
}
