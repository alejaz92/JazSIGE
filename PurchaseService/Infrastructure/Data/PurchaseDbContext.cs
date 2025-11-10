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
        public DbSet<PurchaseDocument> PurchaseDocuments { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----- Purchase_Article: composite PK -----
            modelBuilder.Entity<Purchase_Article>()
                .HasKey(pa => new { pa.PurchaseId, pa.ArticleId });

            modelBuilder.Entity<Purchase_Article>()
                .HasOne(pa => pa.Purchase)
                .WithMany(p => p.Articles)
                .HasForeignKey(pa => pa.PurchaseId);

            // Opcional: precisión para costos (puede ajustarse si usás otra convención global)
            modelBuilder.Entity<Purchase_Article>()
                .Property(pa => pa.UnitCost)
                .HasPrecision(18, 4);

            // ----- PurchaseDocument: relaciones y restricciones -----
            modelBuilder.Entity<PurchaseDocument>()
                .HasOne(d => d.Purchase)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseDocument>()
                .Property(d => d.Currency)
                .HasMaxLength(3)
                .IsRequired();

            modelBuilder.Entity<PurchaseDocument>()
                .Property(d => d.Number)
                .HasMaxLength(64)
                .IsRequired();

            modelBuilder.Entity<PurchaseDocument>()
                .Property(d => d.FileUrl)
                .HasMaxLength(512)
                .IsRequired();

            modelBuilder.Entity<PurchaseDocument>()
                .Property(d => d.FxRate)
                .HasPrecision(18, 6);

            modelBuilder.Entity<PurchaseDocument>()
                .Property(d => d.TotalAmount)
                .HasPrecision(18, 2);

            // Unicidad básica para evitar duplicados exactos (por compra)
            modelBuilder.Entity<PurchaseDocument>()
                .HasIndex(d => new { d.PurchaseId, d.Type, d.Number })
                .IsUnique();

            // ÚNICA FACTURA por compra (índice único filtrado por Type = Invoice)
            // Nota: el filtro usa el valor entero del enum (Invoice = 1).
            modelBuilder.Entity<PurchaseDocument>()
                .HasIndex(d => d.PurchaseId)
                .IsUnique()
                .HasFilter("[Type] = 1");

        }
    }
}