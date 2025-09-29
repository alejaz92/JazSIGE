namespace AccountingService.Infrastructure.Interfaces
{
    public interface INumberingSequenceRepository
    {
        /// Devuelve el próximo número para el scope y avanza el contador de forma atómica.
        Task<int> GetNextAsync(string scope, CancellationToken ct = default);
    }
}
