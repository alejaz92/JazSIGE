
namespace CatalogService.Business.Interfaces
{
    public interface ISupplierValidatorService
    {
        Task<int> ActiveSuppliersByIVAType(int ivaTypeId);
        Task<int> ActiveSuppliersBySellCondition(int sellConditionId);
    }
}
