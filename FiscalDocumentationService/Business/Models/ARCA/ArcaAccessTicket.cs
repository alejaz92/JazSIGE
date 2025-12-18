namespace FiscalDocumentationService.Business.Models.ARCA
{
    public class ArcaAccessTicket
    {
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;

        // UTC recommended
        public DateTime ExpirationTimeUtc { get; set; }

        public bool IsValid(TimeSpan? safetyWindow = null)
        {
            var window = safetyWindow ?? TimeSpan.FromMinutes(2);
            return DateTime.UtcNow < ExpirationTimeUtc.Subtract(window);
        }
    }
}
