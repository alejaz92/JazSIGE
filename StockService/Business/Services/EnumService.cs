using StockService.Business.Interfaces;
using StockService.Business.Models;
using StockService.Infrastructure.Models;

namespace StockService.Business.Services
{
    public class EnumService : IEnumService
    {
        public IEnumerable<EnumDTO> GetStockMovementTypes() => Enum.GetValues(typeof(StockMovementType))
             .Cast<StockMovementType>()
             .Select(e => new EnumDTO
             {
                 Name = e.ToString(),
                 Value = (int)e
             });
    }
}
