using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;

namespace SalesService.Business.Services.Clients
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _userBaseUrl;

        public UserServiceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _userBaseUrl = configuration["GatewayService:UserBaseUrl"];
        }

        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            return client;
        }

        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"{_userBaseUrl}{userId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<UserDTO>();
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync(_userBaseUrl);
            if (!response.IsSuccessStatusCode) return new List<UserDTO>();
            return await response.Content.ReadFromJsonAsync<List<UserDTO>>() ?? new List<UserDTO>();
        }
    }
}
