using SalesService.Business.Models.SalesOrder;

namespace SalesService.Business.Interfaces
{
    public interface ISalesOrderService
    {
        Task<SalesOrderDTO> CreateAsync(SalesOrderCreateDTO dto);
        Task<IEnumerable<SalesOrderListDTO>> GetAllAsync();
        Task<SalesOrderDTO> GetByIdAsync(int id);
    }
}
