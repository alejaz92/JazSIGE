using FiscalDocumentationService.Business.Models.Arca;

namespace FiscalDocumentationService.Business.Interfaces.Clients
{
    public interface IArcaAccessTicketCache
    {
        void Clear(string serviceName);
        ArcaAccessTicket? Get(string serviceName);
        SemaphoreSlim GetLock(string serviceName);
        void Set(string serviceName, ArcaAccessTicket ticket);
    }
}
