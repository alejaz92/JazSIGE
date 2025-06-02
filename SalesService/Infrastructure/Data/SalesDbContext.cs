using Microsoft.EntityFrameworkCore;
using SalesService.Infrastructure.Models;
using SalesService.Infrastructure.Models.SalesOrder;
using SalesService.Infrastructure.Models.SalesQuote;

namespace SalesService.Infrastructure.Data
{
    public class SalesDbContext : DbContext
    {
        public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
        {
        }

        //article prices
        public DbSet<ArticlePriceList> ArticlePriceLists { get; set; } = null!;

        //salesQuote
        public DbSet<SalesQuote> SalesQuotes { get; set; } = null!;
        public DbSet<SalesQuote_Article> SalesQuoteArticles { get; set; } = null!;

        //salesOrder
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrder_Article> SalesOrder_Articles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //salesQuote
            modelBuilder.Entity<SalesQuote>()
                .HasMany(sq => sq.Articles)
                .WithOne(a => a.SalesQuote)
                .HasForeignKey(a => a.SalesQuoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SalesQuote_Article>()
                .Property(a => a.UnitPriceUSD)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<SalesQuote_Article>()
                .Property(a => a.TotalUSD)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<SalesQuote>()
                .Property(sq => sq.SubtotalUSD)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<SalesQuote>()
                .Property(sq => sq.IVAAmountUSD)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<SalesQuote>()
                .Property(sq => sq.TotalUSD)
                .HasColumnType("decimal(18,4)");

            // salesOrder
            modelBuilder.Entity<SalesOrder>()
                .HasMany(o => o.Articles)
                .WithOne(a => a.SalesOrder)
                .HasForeignKey(a => a.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
