using PurchaseService.Business.Models;

namespace PurchaseService.Business.Interfaces
{
    public interface IDispatchService
    {
        Task<int> CreateAsync(DispatchCreateDTO dto, int userId, int purchaseId);
        Task<(IEnumerable<DispatchDTO> Items, int TotalCount)> GetAllAsync(int page, int pageSize);
        Task<IEnumerable<DispatchDTO>> GetAllAsync();
        Task<DispatchDTO?> GetByIdAsync(int id);
    }
}
