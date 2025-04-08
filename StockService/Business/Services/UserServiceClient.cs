using StockService.Business.Interfaces;
using StockService.Business.Models;

namespace StockService.Business.Services
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _userServiceBaseUrl;
        public UserServiceClient(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _userServiceBaseUrl = configuration["GatewayService:UserBaseUrl"];
        }
        public async Task<string?> GetUserNameAsync(int userId)
        {
            var client = _httpClientFactory.CreateClient();

            // Extraer el token JWT del contexto HTTP
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            // Agregar el token al encabezado de la solicitud
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }

            var response = await client.GetFromJsonAsync<UserDTO>($"{_userServiceBaseUrl}{userId}");
            if (response == null) return null;

            return $"{response.FirstName} {response.LastName}";
        }
    }
}
