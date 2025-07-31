namespace FiscalDocumentationService.Business.Models.Arca
{
    public class ArcaResponseDTO
    {
        public string result { get; set; } = "A";  // A = Approved, R = Rejected
        public string cae { get; set; } = string.Empty;
        public string caeExpirationDate { get; set; } = string.Empty; // Format: YYYYMMDD
        public List<ArcaObservation>? observations { get; set; }
        public List<ArcaError>? errors { get; set; }
    }

    public class ArcaObservation
    {
        public int code { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class ArcaError
    {
        public int code { get; set; }
        public string message { get; set; } = string.Empty;
    }
}
