namespace FiscalDocumentationService.Business.Models.Diagnostics
{
    public class ArcaDiagnosticsResponseDTO
    {
        public string EffectiveMode { get; set; } = "Unknown";

        public CompanyFiscalDiagnosticsDTO CompanyFiscal { get; set; } = new ();
        public ArcaAppSettingsDiagnosticsDTO AppSettings { get; set; } = new ();

        public List<string> BlockingReasons { get; set; } = new ();
        public List<string> Warnings { get; set; } = new ();
    }

    public class CompanyFiscalDiagnosticsDTO
    {
        public bool ArcaEnabled { get; set; }
        public string? ArcaEnvironment { get; set; }
        public int? ArcaPointOfSale { get; set; }

        public string? TaxIdMasked { get; set; }
        public string? EmissionProvider { get; set; } // si lo tenés en CompanyFiscalSettingsDTO

        public List<string>? ArcaInvoiceTypesEnabled { get; set; } // si existe en tu DTO
    }

    public class ArcaAppSettingsDiagnosticsDTO
    {
        public string? ArcaEnvironment { get; set; }

        public bool WsaaConfigured { get; set; }
        public bool WsfeConfigured { get; set; }

        public bool CertificatePathConfigured { get; set; }
        public bool CertificateFileExists { get; set; }
        public bool CertificatePasswordConfigured { get; set; }
    }
}
