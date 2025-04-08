using StockService.Business.Models;

namespace StockService.Business.Interfaces
{
    public interface IEnumService
    {
        IEnumerable<EnumDTO> GetStockMovementTypes();
    }
}
