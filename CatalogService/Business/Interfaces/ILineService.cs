using CatalogService.Business.Models.Line;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface ILineService : IGenericService<Line, LineDTO, LineCreateDTO>
    {
        
        Task<IEnumerable<LineDTO>> GetByLineGroupIdAsync(int lineGroupId);
    }
}
