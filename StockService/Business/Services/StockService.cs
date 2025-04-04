using StockService.Business.Interfaces;
using StockService.Infrastructure.Interfaces;

namespace StockService.Business.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IStockMovementRepository _stockMovementRepository;
        public StockService(IStockRepository stockRepository, IStockMovementRepository stockMovementRepository)
        {
            _stockRepository = stockRepository;
            _stockMovementRepository = stockMovementRepository;
        }
    }


}
