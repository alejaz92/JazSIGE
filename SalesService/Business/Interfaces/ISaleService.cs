using SalesService.Business.Models.Sale;

namespace SalesService.Business.Interfaces
{
    public interface ISaleService
    {
        Task<SaleDetailDTO> CreateAsync(SaleCreateDTO dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<SaleDTO>> GetAllAsync();
        Task<SaleDetailDTO?> GetByIdAsync(int id);
    }
}
