namespace FiscalDocumentationService.Business.Models.Clients
{
    public sealed class AccessTicketResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
        public DateTime ExpirationTimeUtc { get; set; }
    }
}
