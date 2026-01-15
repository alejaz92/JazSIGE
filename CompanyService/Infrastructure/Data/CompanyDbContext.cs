using CompanyService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CompanyService.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework database context for Company Service
    /// Manages database connections and entity configurations
    /// </summary>
    public class CompanyDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the CompanyDbContext
        /// </summary>
        /// <param name="options">DbContext options</param>
        public CompanyDbContext(DbContextOptions<CompanyDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Company information entity set
        /// Represents the company data stored in the database
        /// </summary>
        public DbSet<CompanyInfo> CompanyInfo { get; set; }
        
        /// <summary>
        /// Configures the entity model and relationships
        /// Sets up constraints, indexes, and default values
        /// </summary>
        /// <param name="modelBuilder">Model builder instance</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyInfo>(entity =>
            {
                // Primary key configuration
                entity.HasKey(c => c.Id);
                
                // String length constraints for data validation
                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(c => c.ShortName)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(c => c.TaxId)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(c => c.Address)
                    .IsRequired()
                    .HasMaxLength(500);
                
                entity.Property(c => c.PostalCode)
                    .HasMaxLength(20);
                
                entity.Property(c => c.City)
                    .HasMaxLength(100);
                
                entity.Property(c => c.Province)
                    .HasMaxLength(100);
                
                entity.Property(c => c.Country)
                    .HasMaxLength(100);
                
                entity.Property(c => c.Phone)
                    .HasMaxLength(50);
                
                entity.Property(c => c.Email)
                    .HasMaxLength(200);
                
                entity.Property(c => c.LogoUrl)
                    .HasMaxLength(500);
                
                entity.Property(c => c.IVAType)
                    .HasMaxLength(100);
                
                entity.Property(c => c.GrossIncome)
                    .HasMaxLength(50);
                
                entity.Property(c => c.ArcaInvoiceTypesEnabled)
                    .HasMaxLength(50)
                    .HasDefaultValue("1,6");
                
                // Unique index on TaxId for data integrity
                entity.HasIndex(c => c.TaxId)
                    .IsUnique()
                    .HasDatabaseName("IX_CompanyInfo_TaxId");
                
                // Default values for ARCA configuration
                entity.Property(c => c.ArcaEnabled)
                    .HasDefaultValue(false);
                
                entity.Property(c => c.ArcaEnvironment)
                    .HasDefaultValue(ArcaEnvironment.Homologation);
                
                // Timestamp defaults using SQL Server function
                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                
                entity.Property(c => c.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
