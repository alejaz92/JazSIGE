namespace CatalogService.Business.Interfaces
{
    public interface ILineValidatorService
    {
        Task<int> ActiveLinesByLineGroup(int lineGroupId);
    }
}
