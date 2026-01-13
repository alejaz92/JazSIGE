using FiscalDocumentationService.Business.Models.Arca;

namespace FiscalDocumentationService.Business.Interfaces.Clients
{
    public interface IArcaAuthClient
    {
        Task<ArcaAccessTicket> GetAccessTicketAsync(string serviceName);
    }
}
