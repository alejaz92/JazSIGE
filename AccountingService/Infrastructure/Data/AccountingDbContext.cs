using AccountingService.Infrastructure.Models.Common;
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
        public DbSet<NumberingSequence> NumberingSequences { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("ledger");

            // ========= Documents (LedgerDocument) =========
            modelBuilder.Entity<LedgerDocument>(b =>
            {
                b.ToTable("Documents");
                b.HasKey(x => x.Id);

                // ---- Nuevos campos genéricos de origen ----
                b.Property(x => x.SourceKind)        // enum nullable
                 .HasConversion<byte?>();

                b.Property(x => x.SourceDocumentId); // bigint nullable

                b.Property(x => x.DisplayNumber)
                 .HasMaxLength(50);                  // número legible (factura o recibo), nullable

                // ---- Enlace local a Receipt (1:0..1) ----
                b.Property(x => x.ReceiptId);
                b.HasOne(x => x.Receipt)
                 .WithMany()                         // sin navegación inversa por ahora
                 .HasForeignKey(x => x.ReceiptId)
                 .OnDelete(DeleteBehavior.Restrict); // no cascada: el borrado es lógico (Voided)

                // ---- Otros campos ----
                b.Property(x => x.Currency)
                 .IsRequired()
                 .HasMaxLength(3);

                b.Property(x => x.FxRate)
                 .HasPrecision(18, 6);

                b.Property(x => x.TotalOriginal)
                 .HasPrecision(18, 2);

                b.Property(x => x.TotalBase)
                 .HasPrecision(18, 2);

                b.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("SYSUTCDATETIME()");

                // ---- Índices ----

                // (ANTES) Única por FiscalDocumentId —-> SE ELIMINA para no forzar fiscal-only.
                // b.HasIndex(x => x.FiscalDocumentId).IsUnique();  // <-- eliminado

                // Nuevo: unicidad por origen genérico (fiscal o recibo)
                b.HasIndex(x => new { x.SourceKind, x.SourceDocumentId })
                 .IsUnique()
                 .HasDatabaseName("UX_Documents_SourceKind_SourceId");

                // Único por recibo (cada recibo genera a lo sumo 1 documento espejo)
                b.HasIndex(x => x.ReceiptId)
                 .IsUnique()
                 .HasFilter("[ReceiptId] IS NOT NULL")
                 .HasDatabaseName("UX_Documents_ReceiptId");

                // Búsquedas típicas por tercero/estado/tipo con fecha para ordenar
                b.HasIndex(x => new { x.PartyType, x.PartyId, x.Status, x.Kind, x.DocumentDate })
                 .HasDatabaseName("IX_Documents_Party_Status_Kind_Date");
            });

            // ========= Receipts =========
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

            // ========= ReceiptPayments (PaymentLine) =========
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

            // ========= Allocations =========
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
