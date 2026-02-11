using System.Net.Http.Json;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;

namespace PurchaseService.Business.Services
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _userBaseUrl;

        public UserServiceClient(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _userBaseUrl = configuration["GatewayService:UserBaseUrl"];
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string?> GetUserNameAsync(int userId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetFromJsonAsync<UserDTO>($"{_userBaseUrl}{userId}");
            return response != null ? $"{response.FirstName} {response.LastName}" : null;
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetFromJsonAsync<List<UserDTO>>($"{_userBaseUrl}");
            return response ?? new List<UserDTO>();
        }



        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);

            return client;
        }

        
    }
}
