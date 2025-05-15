using Microsoft.EntityFrameworkCore;
using PurchaseService.Infrastructure.Models;

namespace PurchaseService.Infrastructure.Data
{
    public class PurchaseDbContext : DbContext
    {
        public PurchaseDbContext(DbContextOptions<PurchaseDbContext> options) : base(options)
        {
        }
        public DbSet<Purchase> Purchases { get; set; } = null!;
        public DbSet<Dispatch> Dispatches { get; set; } = null!;
        public DbSet<Purchase_Article> PurchaseArticles { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // clave compuesta para Purchase_Article
            modelBuilder.Entity<Purchase_Article>()
                .HasKey(pa => new { pa.PurchaseId, pa.ArticleId });

            modelBuilder.Entity<Purchase_Article>()
                .HasOne(pa => pa.Purchase)
                .WithMany(p => p.Articles)
                .HasForeignKey(pa => pa.PurchaseId);

            //// relacion 1 a 1 entre purchase y dispatch
            //modelBuilder.Entity<Dispatch>()
            //    .HasOne(d => d.Purchase)
            //    .WithOne(p => p.Dispatch)
            //    .HasForeignKey<Dispatch>(d => d.PurchaseId);
        }
    }
}
