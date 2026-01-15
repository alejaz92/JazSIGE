using CompanyService.Infrastructure.Models;

namespace CompanyService.Business.Models
{
    /// <summary>
    /// Data Transfer Object for company fiscal settings
    /// Contains ARCA configuration for invoice generation
    /// </summary>
    public class CompanyFiscalSettingsDTO
    {
        /// <summary>
        /// Tax identification number (CUIT/CUIL)
        /// </summary>
        public string TaxId { get; set; } = string.Empty;

        /// <summary>
        /// Safety switch: if false, real invoices should NOT be issued
        /// </summary>
        public bool ArcaEnabled { get; set; }
        
        /// <summary>
        /// ARCA environment: "Homologation" or "Production"
        /// </summary>
        public string ArcaEnvironment { get; set; } = "Production";
        
        /// <summary>
        /// ARCA point of sale number (nullable)
        /// </summary>
        public int? ArcaPointOfSale { get; set; }
        
        /// <summary>
        /// Comma-separated list of enabled invoice types (e.g., "1,6")
        /// </summary>
        public string ArcaInvoiceTypesEnabled { get; set; } = "1,6";
    }
}
