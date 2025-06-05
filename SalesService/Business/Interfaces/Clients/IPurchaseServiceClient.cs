using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface IPurchaseServiceClient
    {
        Task<DispatchDTO> GetDispatchByIdAsync(int dispatchId); 
    }
}
