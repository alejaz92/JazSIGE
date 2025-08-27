using SalesService.Business.Models.Rates;

namespace SalesService.Business.Interfaces
{
    public interface IRatesService
    {
        Task<ExchangeRateDTO> GetUsdArsOficialAsync();
    }
}
