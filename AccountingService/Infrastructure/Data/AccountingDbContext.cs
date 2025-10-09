
using AccountingService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Data
{
    public class AccountingDbContext : DbContext
    {
        public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options)
        {
        }

        public DbSet<LedgerDocument> LedgerDocuments => Set<LedgerDocument>();
        public DbSet<Receipt> Receipts => Set<Receipt>();
        public DbSet<ReceiptPayment> ReceiptPayments => Set<ReceiptPayment>();
        public DbSet<ReceiptAllocation> ReceiptAllocations => Set<ReceiptAllocation>();
        public DbSet<AllocationBatch> AllocationBatches => Set<AllocationBatch>();
        public DbSet<AllocationItem> AllocationItems => Set<AllocationItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enums
            modelBuilder.Entity<LedgerDocument>().Property(p => p.PartyType).HasConversion<int>();
            modelBuilder.Entity<LedgerDocument>().Property(p => p.Kind).HasConversion<int>();
            modelBuilder.Entity<LedgerDocument>().Property(p => p.Status).HasConversion<int>();
            modelBuilder.Entity<ReceiptPayment>().Property(p => p.Method).HasConversion<int>();

            // RowVersion
            modelBuilder.Entity<LedgerDocument>().Property(p => p.RowVersion).IsRowVersion();
            modelBuilder.Entity<Receipt>().Property(p => p.RowVersion).IsRowVersion();
            modelBuilder.Entity<ReceiptPayment>().Property(p => p.RowVersion).IsRowVersion();
            modelBuilder.Entity<ReceiptAllocation>().Property(p => p.RowVersion).IsRowVersion();
            modelBuilder.Entity<AllocationBatch>().Property(p => p.RowVersion).IsRowVersion();
            modelBuilder.Entity<AllocationItem>().Property(p => p.RowVersion).IsRowVersion();

            // Decimal precisions
            modelBuilder.Entity<LedgerDocument>().Property(p => p.FxRate).HasColumnType("decimal(18,6)");
            modelBuilder.Entity<LedgerDocument>().Property(p => p.AmountOriginal).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LedgerDocument>().Property(p => p.AmountARS).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LedgerDocument>().Property(p => p.PendingARS).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ReceiptPayment>().Property(p => p.FxRate).HasColumnType("decimal(18,6)");
            modelBuilder.Entity<ReceiptPayment>().Property(p => p.AmountOriginal).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ReceiptPayment>().Property(p => p.AmountARS).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ReceiptAllocation>().Property(p => p.AppliedARS).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<AllocationItem>().Property(p => p.AppliedARS).HasColumnType("decimal(18,2)");

            // Required cortos
            modelBuilder.Entity<LedgerDocument>().Property(p => p.AmountARS).IsRequired();
            modelBuilder.Entity<LedgerDocument>().Property(p => p.PendingARS).IsRequired();
            modelBuilder.Entity<Receipt>().Property(p => p.Number).HasMaxLength(50);
            modelBuilder.Entity<LedgerDocument>().Property(p => p.Currency).HasMaxLength(3);

            // Índices
            modelBuilder.Entity<LedgerDocument>()
                .HasIndex(p => new { p.Kind, p.ExternalRefId })
                .IsUnique(); // idempotencia por origen

            modelBuilder.Entity<LedgerDocument>()
                .HasIndex(p => new { p.PartyType, p.PartyId, p.DocumentDate });

            modelBuilder.Entity<LedgerDocument>()
                .HasIndex(p => new { p.Kind, p.Status });

            modelBuilder.Entity<ReceiptAllocation>()
                .HasIndex(a => a.TargetDocumentId);

            modelBuilder.Entity<AllocationBatch>()
                .HasMany(b => b.Items)
                .WithOne(i => i.AllocationBatch)
                .HasForeignKey(i => i.AllocationBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AllocationBatch>()
                .HasIndex(b => b.TargetDocumentId);

            // Relaciones Receipt
            modelBuilder.Entity<ReceiptPayment>()
                .HasOne(p => p.Receipt)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReceiptAllocation>()
                .HasOne(a => a.Receipt)
                .WithMany(r => r.Allocations)
                .HasForeignKey(a => a.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }   
        
        
    }
}
