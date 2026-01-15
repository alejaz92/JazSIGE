using CatalogService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
        {
        }
        
        public DbSet<Article> Articles { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<GrossIncomeType> GrossIncomeTypes { get; set; }
        public DbSet<IVAType> IVATypes { get; set; }
        public DbSet<Line> Lines { get; set; }
        public DbSet<LineGroup> LineGroups { get; set; }
        public DbSet<PostalCode> PostalCodes { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<SellCondition> SellConditions { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Transport> Transports { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }



        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>()
                .HasOne(a => a.Brand)
                .WithMany(b => b.Articles)
                .HasForeignKey(a => a.BrandId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Article>()
                .HasOne(a => a.Line)
                .WithMany(l => l.Articles)
                .HasForeignKey(a => a.LineId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Article>()
                .HasOne(a => a.Unit)
                .WithMany(b => b.Articles)
                .HasForeignKey(a => a.UnitId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Article>()
                .HasOne(a => a.GrossIncomeType)
                .WithMany(b => b.Articles)
                .HasForeignKey(a => a.GrossIncomeTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<City>()
                .HasOne(c => c.Province)
                .WithMany(p => p.Cities)
                .HasForeignKey(c => c.ProvinceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PostalCode>()
                .HasOne(p => p.City)
                .WithMany(c => c.PostalCodes)
                .HasForeignKey(p => p.CityId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Province>()
                .HasOne(p => p.Country)
                .WithMany(c => c.Provinces)
                .HasForeignKey(p => p.CountryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Line>()
                .HasOne(l => l.LineGroup)
                .WithMany(lg => lg.Lines)
                .HasForeignKey(l => l.LineGroupId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Transport>()
                .HasOne(t => t.PostalCode)
                .WithMany(c => c.Transports)
                .HasForeignKey(t => t.PostalCodeId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<IVAType>(entity =>
            {
                entity.Property(x => x.Description)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.ArcaCode)
                    .IsRequired();

                entity.HasIndex(x => x.Description).IsUnique();
                entity.HasIndex(x => x.ArcaCode).IsUnique();

                // Seed fijo (valores ARCA/AFIP)
                var seedDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

                entity.HasData(
                    new IVAType { Id = 1, Description = "IVA Responsable Inscripto", ArcaCode = 1, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
                    new IVAType { Id = 2, Description = "IVA Sujeto Exento", ArcaCode = 4, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
                    new IVAType { Id = 3, Description = "Consumidor Final", ArcaCode = 5, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
                    new IVAType { Id = 4, Description = "Responsable Monotributo", ArcaCode = 6, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
                    new IVAType { Id = 5, Description = "Sujeto No Categorizado", ArcaCode = 7, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
                    new IVAType { Id = 6, Description = "Proveedor del Exterior", ArcaCode = 8, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
                    new IVAType { Id = 7, Description = "Cliente del Exterior", ArcaCode = 9, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
                    new IVAType { Id = 8, Description = "IVA Liberado – Ley 19.640", ArcaCode = 10, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
                    new IVAType { Id = 9, Description = "Monotributista Social", ArcaCode = 13, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
                    new IVAType { Id = 10, Description = "IVA No Alcanzado", ArcaCode = 15, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate },
                    new IVAType { Id = 11, Description = "Monotributo Trabajador Independiente Promovido", ArcaCode = 16, IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate }
                );
            });



            // precision
            modelBuilder.Entity<Article>()
                .Property(a => a.IVAPercentage)
                .HasPrecision(18, 2); // Ajustá la precisión según necesidad



        }
    }
}
