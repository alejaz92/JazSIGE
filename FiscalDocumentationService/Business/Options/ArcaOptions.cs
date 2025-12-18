namespace FiscalDocumentationService.Business.Options
{
    public class ArcaOptions
    {
        public string Environment { get; set; } = "Homologation";
        public WsaaOptions Wsaa { get; set; } = new ();
        public CertificateOptions Certificate { get; set; } = new ();
    }

    public class WsaaOptions
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
