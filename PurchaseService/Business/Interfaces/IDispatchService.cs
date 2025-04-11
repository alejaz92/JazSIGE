using PurchaseService.Business.Models;

namespace PurchaseService.Business.Interfaces
{
    public interface IDispatchService
    {
        Task<(IEnumerable<DispatchDTO> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
        Task<IEnumerable<DispatchDTO>> GetAllAsync();
        Task<DispatchDTO?> GetByIdAsync(int id);
    }
}
