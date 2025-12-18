using FiscalDocumentationService.Business.Models.ARCA;

namespace FiscalDocumentationService.Business.Interfaces.Clients
{
    public interface IArcaAuthClient
    {
        Task<ArcaAccessTicket> GetAccessTicketAsync(string serviceName);
    }
}
