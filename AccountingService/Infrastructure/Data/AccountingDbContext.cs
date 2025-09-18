using AccountingService.Infrastructure.Models.Ledger;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Data
{
    public class AccountingDbContext : DbContext
    {
        public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options)
        {           
        }
        public DbSet<LedgerDocument> LedgerDocuments { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<PaymentLine> PaymentLines { get; set; }
        public DbSet<Allocation> Allocations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("ledger");

            modelBuilder.Entity<LedgerDocument>(b =>
            {
                b.ToTable("Documents");
                b.HasKey(x => x.Id);

                b.Property(x => x.FiscalDocumentId).IsRequired().HasMaxLength(50);
                b.Property(x => x.FiscalDocumentNumber).IsRequired().HasMaxLength(50);

                b.Property(x => x.Currency).IsRequired().HasMaxLength(3);
                b.Property(x => x.FxRate).HasPrecision(18, 6);
                b.Property(x => x.TotalOriginal).HasPrecision(18, 2);
                b.Property(x => x.TotalBase).HasPrecision(18, 2);
                b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                // Única por documento fiscal (evita duplicar la misma factura/NC/ND)
                b.HasIndex(x => x.FiscalDocumentId).IsUnique();

                // Búsquedas típicas por tercero/estado/tipo
                b.HasIndex(x => new { x.PartyType, x.PartyId, x.Status, x.Kind });
              
            });

            // CashReceipt
            modelBuilder.Entity<Receipt>(b =>
            {
                b.ToTable("Receipts");
                b.HasKey(x => x.Id);

                b.Property(x => x.Number).HasMaxLength(30);
                b.Property(x => x.Currency).IsRequired().HasMaxLength(3);
                b.Property(x => x.FxRate).HasPrecision(18, 6);
                b.Property(x => x.TotalOriginal).HasPrecision(18, 2);
                b.Property(x => x.TotalBase).HasPrecision(18, 2);
                b.Property(x => x.Notes).HasMaxLength(500);

                b.HasIndex(x => new { x.PartyType, x.PartyId, x.Date });
            });

            // PaymentLine
            modelBuilder.Entity<PaymentLine>(b =>
            {
                b.ToTable("ReceiptPayments");
                b.HasKey(x => x.Id);

                b.Property(x => x.AmountOriginal).HasPrecision(18, 2);
                b.Property(x => x.AmountBase).HasPrecision(18, 2);

                b.Property(x => x.TransactionReference).HasMaxLength(100);
                b.Property(x => x.Notes).HasMaxLength(100);

                b.HasIndex(x => x.ReceiptId);

                b.HasOne(x => x.Receipt)
                 .WithMany(r => r.PaymentLines)
                 .HasForeignKey(x => x.ReceiptId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Allocation
            modelBuilder.Entity<Allocation>(b =>
            {
                b.ToTable("Allocations");
                b.HasKey(x => x.Id);
                b.Property(x => x.AmountBase).HasPrecision(18, 2);
                b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

                b.HasOne(x => x.Receipt)
                 .WithMany(r => r.Allocations)
                 .HasForeignKey(x => x.ReceiptId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.DebitDocument)
                 .WithMany()
                 .HasForeignKey(x => x.DebitDocumentId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => x.ReceiptId);
                b.HasIndex(x => x.DebitDocumentId);
            });

        }
    }
}
