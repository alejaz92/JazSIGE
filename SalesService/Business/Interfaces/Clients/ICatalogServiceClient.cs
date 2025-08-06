using SalesService.Business.Models.Article_PriceLists;
using SalesService.Business.Models.Clients;

namespace SalesService.Business.Interfaces.Clients
{
    public interface ICatalogServiceClient
    {
        Task<ArticleDTO?> GetArticleByIdAsync(int articleId);
        Task<CustomerDTO?> GetCustomerByIdAsync(int customerId);
        Task<PostalCodeDTO?> GetPostalCodeByIdAsync(int postalCodeId);
        Task<PriceListDTO?> GetPriceListByIdAsync(int priceListId);
        Task<IEnumerable<PriceListDTO>> GetPriceLists();
        Task<TransportDTO?> GetTransportByIdAsync(int transportId);
        Task<WarehouseDTO?> GetWarehouseByIdAsync(int warehouseId);
    }
}
