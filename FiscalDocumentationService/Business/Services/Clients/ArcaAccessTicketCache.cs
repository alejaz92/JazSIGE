using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.ARCA;
using System.Collections.Concurrent;

namespace FiscalDocumentationService.Business.Services.Clients
{
    public class ArcaAccessTicketCache : IArcaAccessTicketCache
    {
        private readonly ConcurrentDictionary<string, ArcaAccessTicket> _cache = new();

        public ArcaAccessTicket? Get(string serviceName)
            => _cache.TryGetValue(serviceName, out var t) ? t : null;

        public void Set(string serviceName, ArcaAccessTicket ticket)
            => _cache[serviceName] = ticket;

        public void Clear(string serviceName)
            => _cache.TryRemove(serviceName, out _);
    }
}
