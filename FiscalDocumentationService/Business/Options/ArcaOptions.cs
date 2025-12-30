namespace FiscalDocumentationService.Business.Options
{
    public class ArcaOptions
    {
        public string Environment { get; set; } = "Homologation";
        public bool AllowProductionEmission { get; set; } = false;

        public WsaaOptions Wsaa { get; set; } = new();
        public WsfeOptions Wsfe { get; set; } = new();
        public CertificateOptions Certificate { get; set; } = new();
    }

    public class WsaaOptions
    {
        public string HomologationUrl { get; set; } = string.Empty;
        public string ProductionUrl { get; set; } = string.Empty;
    }

    public class WsfeOptions
    {
        public string HomologationUrl { get; set; } = string.Empty;
        public string ProductionUrl { get; set; } = string.Empty;
    }

    public class  CertificateOptions 
    {
        public string PfxPath { get; set; } = string.Empty;
        public string PfxPassword { get; set; } = string.Empty;
    }
}
