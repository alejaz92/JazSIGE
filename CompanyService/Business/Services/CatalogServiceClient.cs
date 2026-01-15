using CompanyService.Business.Interfaces;
using CompanyService.Business.Models;
using CompanyService.Business.Exceptions;
using System.Net;

namespace CompanyService.Business.Services
{
    /// <summary>
    /// HTTP client for communicating with the Catalog Service
    /// Retrieves postal codes and IVA types from the external catalog service
    /// </summary>
    public class CatalogServiceClient : ICatalogServiceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CatalogServiceClient> _logger;
        private readonly string _catalogBaseUrl;
        private const int TimeoutSeconds = 30;

        /// <summary>
        /// Initializes a new instance of the CatalogServiceClient
        /// </summary>
        /// <param name="httpClientFactory">HTTP client factory for creating HTTP clients</param>
        /// <param name="httpContextAccessor">HTTP context accessor for retrieving authorization token</param>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger instance</param>
        /// <exception cref="InvalidOperationException">Thrown when CatalogBaseUrl is not configured</exception>
        public CatalogServiceClient(
            IHttpClientFactory httpClientFactory, 
            IHttpContextAccessor httpContextAccessor, 
            IConfiguration configuration,
            ILogger<CatalogServiceClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _catalogBaseUrl = configuration["GatewayService:CatalogBaseUrl"] 
                ?? throw new InvalidOperationException("CatalogBaseUrl is not configured");
        }

        /// <summary>
        /// Retrieves postal code information by ID from the catalog service
        /// </summary>
        /// <param name="id">Postal code ID</param>
        /// <returns>Postal code DTO, or null if not found</returns>
        /// <exception cref="ArgumentException">Thrown when id is less than or equal to 0</exception>
        /// <exception cref="Exception">Thrown when HTTP request fails or times out</exception>
        public async Task<PostalCodeDTO?> GetPostalCodeByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Postal code ID must be greater than 0", nameof(id));

            var client = CreateAuthorizedClient();
            try
            {
                var response = await client.GetAsync($"{_catalogBaseUrl}PostalCode/{id}");
                
                // Handle 404 as null return (not found)
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<PostalCodeDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling CatalogService for PostalCode {PostalCodeId}", id);
                throw new Exception($"Failed to retrieve postal code: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout calling CatalogService for PostalCode {PostalCodeId}", id);
                throw new Exception("Request to catalog service timed out", ex);
            }
        }

        /// <summary>
        /// Retrieves IVA type information by ID from the catalog service
        /// </summary>
        /// <param name="id">IVA type ID</param>
        /// <returns>IVA type DTO, or null if not found</returns>
        /// <exception cref="ArgumentException">Thrown when id is less than or equal to 0</exception>
        /// <exception cref="Exception">Thrown when HTTP request fails or times out</exception>
        public async Task<IVATypeDTO?> GetIVATypeByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("IVA type ID must be greater than 0", nameof(id));

            var client = CreateAuthorizedClient();
            try
            {
                var response = await client.GetAsync($"{_catalogBaseUrl}IVAType/{id}");
                
                // Handle 404 as null return (not found)
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<IVATypeDTO>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling CatalogService for IVAType {IVATypeId}", id);
                throw new Exception($"Failed to retrieve IVA type: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout calling CatalogService for IVAType {IVATypeId}", id);
                throw new Exception("Request to catalog service timed out", ex);
            }
        }

        /// <summary>
        /// Creates an authorized HTTP client with JWT token from current request context
        /// </summary>
        /// <returns>Configured HTTP client with authorization header and timeout</returns>
        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
            
            // Forward authorization token from incoming request to catalog service
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Add("Authorization", token);
            
            return client;
        }
    }
}
