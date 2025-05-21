using CompanyService.Business.Models;

namespace CompanyService.Business.Interfaces
{
    public interface ICatalogServiceClient
    {
        Task<PostalCodeDTO?> GetPostalCodeByIdAsync(int id);
        Task<IVATypeDTO?> GetIVATypeByIdAsync(int id);
    }
}
