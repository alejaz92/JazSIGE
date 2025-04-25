using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Customer;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;
using System.Text.Json;

namespace CatalogService.Business.Services
{
    public class CustomerService : GenericService<Customer, CustomerDTO, CustomerCreateDTO>, ICustomerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _gatewayUserServiceUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string? _cachedSellerName;

        public CustomerService(ICustomerRepository repository, IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(repository)
        {
            _httpClientFactory = httpClientFactory;
            _gatewayUserServiceUrl = configuration["GatewayService:BaseUrl"];
            _httpContextAccessor = httpContextAccessor;
        }

        protected override CustomerDTO MapToDTO(Customer entity)
        {
            return new CustomerDTO
            {
                Id = entity.Id,
                TaxId = entity.TaxId,
                CompanyName = entity.CompanyName,
                ContactName = entity.ContactName,
                Address = entity.Address,
                PostalCodeId = entity.PostalCodeId,
                PostalCode = entity.PostalCode.Code,
                CityId = entity.PostalCode.CityId,
                City = entity.PostalCode.City.Name,
                ProvinceId = entity.PostalCode.City.ProvinceId,
                Province = entity.PostalCode.City.Province.Name,
                CountryId = entity.PostalCode.City.Province.CountryId,
                Country = entity.PostalCode.City.Province.Country.Name,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                IVATypeId = entity.IVATypeId,
                IVAType = entity.IVAType.Description,
                WarehouseId = entity.WarehouseId,
                Warehouse = entity.Warehouse.Description,
                TransportId = entity.TransportId,
                Transport = entity.Transport.Name,
                SellConditionId = entity.SellConditionId,
                SellCondition = entity.SellCondition.Description,
                DeliverAddress = entity.DeliverAddress,
                SellerId = entity.SellerId,
                SellerName = _cachedSellerName ?? "Unknown",
                AssignedPriceListId = entity.AssignedPriceListId,
                AssignedPriceList = entity.AssignedPriceList.Description,

            };
        }

        protected override Customer MapToDomain(CustomerCreateDTO dto)
        {
            return new Customer
            {
                TaxId = dto.TaxId,
                CompanyName = dto.CompanyName,
                ContactName = dto.ContactName,
                Address = dto.Address,
                PostalCodeId = dto.PostalCodeId,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                IVATypeId = dto.IVATypeId,
                WarehouseId = dto.WarehouseId,
                TransportId = dto.TransportId,
                SellConditionId = dto.SellConditionId,
                DeliverAddress = dto.DeliverAddress,
                SellerId = dto.SellerId,
                AssignedPriceListId = dto.AssignedPriceListId
            };
        }

        protected override void UpdateDomain(Customer entity, CustomerCreateDTO dto)
        {
            entity.TaxId = dto.TaxId;
            entity.CompanyName = dto.CompanyName;
            entity.ContactName = dto.ContactName;
            entity.Address = dto.Address;
            entity.PostalCodeId = dto.PostalCodeId;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.Email = dto.Email;
            entity.IVATypeId = dto.IVATypeId;
            entity.WarehouseId = dto.WarehouseId;
            entity.TransportId = dto.TransportId;
            entity.SellConditionId = dto.SellConditionId;
            entity.DeliverAddress = dto.DeliverAddress;
            entity.SellerId = dto.SellerId;
            entity.AssignedPriceListId = dto.AssignedPriceListId;
        }

        private async Task<string> GetSellerNameAsync(int sellerId)
        {
            var client = _httpClientFactory.CreateClient();

            // Extraer el token JWT del contexto HTTP
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            // Agregar el token al encabezado de la solicitud
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }

            var response = await client.GetAsync($"{_gatewayUserServiceUrl}{sellerId}");


            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDTO>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";
        }

        public override async Task<string?> ValidateBeforeSave(CustomerCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.CompanyName))
                return "El nombre del cliente es obligatorio.";

            var user = await GetSellerAsync(model.SellerId);
            if (user == null)
                return "Seller Id is not valid.";

            _cachedSellerName = $"{user.FirstName} {user.LastName}";
            return null;
        }

        private async Task<UserDTO?> GetSellerAsync(int sellerId)
        {


            var client = _httpClientFactory.CreateClient();

            // Extraer el token JWT del contexto HTTP
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            // Agregar el token al encabezado de la solicitud
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
            }


            var response = await client.GetAsync($"{_gatewayUserServiceUrl}{sellerId}");
            if(!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDTO>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected override Task<IEnumerable<Customer>> GetAllWithIncludes() => _repository.GetAllIncludingAsync(
            c => c.PostalCode,
            c => c.PostalCode.City,
            c => c.PostalCode.City.Province,
            c => c.PostalCode.City.Province.Country,
            c => c.IVAType,
            c => c.Warehouse,
            c => c.Transport,
            c => c.SellCondition,
            c => c.AssignedPriceList
            );

        protected override Task<Customer> GetWithIncludes(int id) => _repository.GetIncludingAsync(
            id,
            t => t.PostalCode, c => c.PostalCode.City,
            c => c.PostalCode.City.Province,
            c => c.PostalCode.City.Province.Country,
            c => c.IVAType,
            c => c.Warehouse,
            c => c.Transport,
            c => c.SellCondition,
            c => c.AssignedPriceList
            );

    }
}

