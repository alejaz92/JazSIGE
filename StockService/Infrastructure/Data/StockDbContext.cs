using AuthService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using StockService.Infrastructure.Models;

namespace StockService.Infrastructure.Data
{
    public class StockDbContext : DbContext
    {
        public StockDbContext(DbContextOptions<StockDbContext> options)
            : base(options)
        {
        }

        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<StockByDispatch> StockByDispatches { get; set; }
        public DbSet<PendingStockEntry> PendingStockEntries { get; set; }
        public DbSet<CommitedStockEntry> CommitedStockEntries { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Stock>()
                .HasIndex(s => new { s.ArticleId, s.WarehouseId })
                .IsUnique();

            modelBuilder.Entity<Stock>()
                .Property(s => s.Quantity)
                .HasPrecision(18, 4);

            modelBuilder.Entity<StockMovement>()
                .Property(s => s.Quantity)
                .HasPrecision(18, 4);
            modelBuilder.Entity<StockByDispatch>()
                .Property(s => s.Quantity)
                .HasPrecision(18, 4);

            modelBuilder.Entity<PendingStockEntry>()
                .Property(p => p.Quantity)
                .HasPrecision(18, 4);

            modelBuilder.Entity<CommitedStockEntry>()
                .Property(c => c.Quantity)
                .HasPrecision(18, 4);

            modelBuilder.Entity<CommitedStockEntry>()
                .Property(c => c.Delivered)
                .HasPrecision(18, 4);


        }
    }
}
