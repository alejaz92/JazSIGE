namespace CompanyService.Infrastructure.Models
{
    public enum ArcaEnvironment
    {
        Homologation = 0,
        Production = 1
    }

    public class CompanyInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string TaxId { get; set; }
        public string Address { get; set; }
        public int PostalCodeId { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LogoUrl { get; set; }
        public int IVATypeId { get; set; }
        public string IVAType { get; set; }
        public string GrossIncome { get; set; }
        public DateTime DateOfIncorporation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        // ---------------------------
        // ARCA / Fiscal settings
        // ---------------------------

        // Safety switch: si es false, NO se debe emitir real (aunque esté configurado el resto)
        public bool ArcaEnabled { get; set; } = false;

        // Homologation vs Production
        public ArcaEnvironment ArcaEnvironment { get; set; } = ArcaEnvironment.Homologation;

        // Nullable hasta que lo defina la contadora
        public int? ArcaPointOfSale { get; set; }

        // Por ahora A y B => (1 y 6). Lo guardo como string simple para no complejizar DB.
        // Formato: "1,6"
        public string ArcaInvoiceTypesEnabled { get; set; } = "1,6";
    }
}
