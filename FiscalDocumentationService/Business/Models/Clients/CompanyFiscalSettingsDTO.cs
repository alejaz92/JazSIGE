namespace FiscalDocumentationService.Business.Models.Clients
{
    public class CompanyFiscalSettingsDTO
    {
        public string TaxId { get; set; } = string.Empty;

        public bool ArcaEnabled { get; set; }
        public string ArcaEnvironment { get; set; } = "Production";
        public int? ArcaPointOfSale { get; set; }
        public string ArcaInvoiceTypesEnabled { get; set; } = "1,6";
    }
}
