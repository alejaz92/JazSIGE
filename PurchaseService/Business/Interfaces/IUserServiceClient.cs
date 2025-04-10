
namespace PurchaseService.Business.Interfaces
{
    public interface IUserServiceClient
    {
        Task<string?> GetUserNameAsync(int userId);
    }
}
