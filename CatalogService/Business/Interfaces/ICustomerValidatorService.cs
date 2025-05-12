
namespace CatalogService.Business.Interfaces
{
    public interface ICustomerValidatorService
    {
        Task<int> ActiveCustomersByIVAType(int ivaTypeId);
        Task<int> ActiveCustomersByPriceList(int priceListId);
        Task<int> ActiveCustomersBySellCondition(int sellConditionID);
    }
}
