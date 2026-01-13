using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.Arca;
using System.Collections.Concurrent;
using System.Threading;

namespace FiscalDocumentationService.Business.Services.Clients
{
    public class ArcaAccessTicketCache : IArcaAccessTicketCache
    {
        private readonly ConcurrentDictionary<string, ArcaAccessTicket> _cache = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public ArcaAccessTicket? Get(string serviceName)
            => _cache.TryGetValue(serviceName, out var t) ? t : null;

        public void Set(string serviceName, ArcaAccessTicket ticket)
            => _cache[serviceName] = ticket;

        public void Clear(string serviceName)
            => _cache.TryRemove(serviceName, out _);

        public SemaphoreSlim GetLock(string serviceName)
            => _locks.GetOrAdd(serviceName, _ => new SemaphoreSlim(1, 1));
    }
}
