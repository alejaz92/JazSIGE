namespace CompanyService.Infrastructure.Models
{
    /// <summary>
    /// ARCA environment enumeration
    /// Defines the environment for fiscal invoice generation
    /// </summary>
    public enum ArcaEnvironment
    {
        /// <summary>
        /// Homologation/test environment
        /// </summary>
        Homologation = 0,
        
        /// <summary>
        /// Production environment
        /// </summary>
        Production = 1
    }

    /// <summary>
    /// Company information entity
    /// Represents the company that owns the business processes
    /// Used for invoicing and business configuration
    /// </summary>
    public class CompanyInfo
    {
        /// <summary>
        /// Primary key identifier
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Full company name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Short name or abbreviation
        /// </summary>
        public string ShortName { get; set; }
        
        /// <summary>
        /// Tax identification number (CUIT/CUIL)
        /// </summary>
        public string TaxId { get; set; }
        
        /// <summary>
        /// Street address
        /// </summary>
        public string Address { get; set; }
        
        /// <summary>
        /// Postal code ID reference to catalog service
        /// </summary>
        public int PostalCodeId { get; set; }
        
        /// <summary>
        /// Postal code description (denormalized for performance)
        /// </summary>
        public string PostalCode { get; set; }
        
        /// <summary>
        /// City name (denormalized from postal code)
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// Province/State name (denormalized from postal code)
        /// </summary>
        public string Province { get; set; }
        
        /// <summary>
        /// Country name (denormalized from postal code)
        /// </summary>
        public string Country { get; set; }
        
        /// <summary>
        /// Contact phone number (optional)
        /// </summary>
        public string? Phone { get; set; }
        
        /// <summary>
        /// Contact email address (optional)
        /// </summary>
        public string? Email { get; set; }
        
        /// <summary>
        /// URL to company logo image (optional)
        /// </summary>
        public string? LogoUrl { get; set; }
        
        /// <summary>
        /// IVA (VAT) type ID reference to catalog service
        /// </summary>
        public int IVATypeId { get; set; }
        
        /// <summary>
        /// IVA type description (denormalized from catalog)
        /// </summary>
        public string IVAType { get; set; }
        
        /// <summary>
        /// Gross income registration number
        /// </summary>
        public string GrossIncome { get; set; }
        
        /// <summary>
        /// Date when the company was incorporated
        /// </summary>
        public DateTime DateOfIncorporation { get; set; }
        
        /// <summary>
        /// Record creation timestamp (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Record last update timestamp (UTC)
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // ====================================================================
        // ARCA / Fiscal Settings
        // ====================================================================

        /// <summary>
        /// Safety switch for ARCA fiscal invoice generation
        /// If false, real invoices should NOT be issued even if other settings are configured
        /// </summary>
        public bool ArcaEnabled { get; set; } = false;

        /// <summary>
        /// ARCA environment (Homologation or Production)
        /// Determines which ARCA endpoint to use for invoice generation
        /// </summary>
        public ArcaEnvironment ArcaEnvironment { get; set; } = ArcaEnvironment.Homologation;

        /// <summary>
        /// ARCA point of sale number
        /// Nullable until defined by accounting department
        /// </summary>
        public int? ArcaPointOfSale { get; set; }

        /// <summary>
        /// Comma-separated list of enabled invoice types
        /// Format: "1,6" (e.g., Type A=1, Type B=6)
        /// Stored as simple string to avoid database complexity
        /// </summary>
        public string ArcaInvoiceTypesEnabled { get; set; } = "1,6";
    }
}
