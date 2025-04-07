
namespace StockService.Business.Interfaces
{
    public interface IUserServiceClient
    {
        Task<string?> GetUserNameAsync(int userId);
    }
}
