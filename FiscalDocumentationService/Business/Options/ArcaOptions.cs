namespace FiscalDocumentationService.Business.Options
{
    public class ArcaOptions
    {
        public string Environment { get; set; } = "Homologation";
        public bool AllowProductionEmission { get; set; } = false;

        public WsfeOptions Wsfe { get; set; } = new();

        public bool UseDummy { get; set; } = false;
        
        public WsaaFunctionOptions WsaaFunction { get; set; } = new();
    }

    public class WsaaFunctionOptions {
       public string BaseUrl { get; set; } = string.Empty;
       public string FunctionCode { get; set; } = string.Empty;
       public string InternalAccessKey { get; set; } = string.Empty;
    }


    public class WsfeOptions
    {
        public string HomologationUrl { get; set; } = string.Empty;
        public string ProductionUrl { get; set; } = string.Empty;
    }
}
