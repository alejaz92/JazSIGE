using CatalogService.Infrastructure.Models;

namespace CatalogService.Infrastructure.Interfaces
{
    public interface ILineRepository : IGenericRepository<Line>
    {
        Task<IEnumerable<Line>> GetByLineGroupIdAsync(int lineGroupId);
    }
}
