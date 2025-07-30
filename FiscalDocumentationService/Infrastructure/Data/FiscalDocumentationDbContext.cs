using FiscalDocumentationService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace FiscalDocumentationService.Infrastructure.Data
{
    public class FiscalDocumentationDbContext : DbContext
    {
        public FiscalDocumentationDbContext(DbContextOptions<FiscalDocumentationDbContext> options) : base(options) { }

        public DbSet<FiscalDocument> FiscalDocuments { get; set; }
        public DbSet<FiscalDocumentItem> FiscalDocumentItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FiscalDocument>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.HasMany(f => f.Items)
                      .WithOne(i => i.FiscalDocument)
                      .HasForeignKey(i => i.FiscalDocumentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FiscalDocumentItem>(entity =>
            {
                entity.HasKey(i => i.Id);
            });
        }
    }
}
