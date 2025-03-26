using CatalogService.Business.Models.Transport;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Interfaces
{
    public interface ITransportService : IGenericService<Transport, TransportDTO, TransportCreateDTO>
    {
    }
}
