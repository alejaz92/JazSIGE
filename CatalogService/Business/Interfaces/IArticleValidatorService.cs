namespace CatalogService.Business.Interfaces
{
    public interface IArticleValidatorService
    {
        Task<int> ActiveArticlesByBrand(int brandId);
        Task<int> ActiveArticlesByGrossIncomeType(int grossIncomeTypeId);
        Task<int> ActiveArticlesByLine(int lineId);
        Task<int> ActiveArticlesByUnit(int unitId);
    }
}
