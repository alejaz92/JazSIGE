using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.PostalCode;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class PostalCodeService : GenericService<PostalCode, PostalCodeDTO, PostalCodeCreateDTO>, IPostalCodeService
    {
        private readonly IPostalCodeRepository _repository;

        public PostalCodeService(IPostalCodeRepository repository) : base(repository)
        {
            _repository = repository;
        }

        protected override PostalCodeDTO MapToDTO(PostalCode entity)
        {
            return new PostalCodeDTO
            {
                Id = entity.Id,
                Code = entity.Code,
                CityId = entity.CityId,
                City = entity.City.Name,
                ProvinceId = entity.City.ProvinceId,
                Province = entity.City.Province.Name,
                CountryId = entity.City.Province.CountryId,
                Country = entity.City.Province.Country.Name
            };
        }
        protected override PostalCode MapToDomain(PostalCodeCreateDTO dto)
        {
            return new PostalCode
            {
                Code = dto.Code,
                CityId = dto.CityId
            };
        }
        protected override void UpdateDomain(PostalCode entity, PostalCodeCreateDTO dto)
        {
            entity.Code = dto.Code;
            entity.CityId = dto.CityId;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        public async Task<bool> IsPostalCodeUnique(string Code)
        {
            var postalCodes = await _repository.FindAsync(b => b.Code == Code);
            return !postalCodes.Any();
        }
        public override async Task<string?> ValidateBeforeSave(PostalCodeCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Code))
                return "Postal Code is mandatory.";
            var isUnique = await IsPostalCodeUnique(model.Code);
            if (!isUnique)
                return "Postal Code must be unique.";
            return null;
        }

        protected override Task<IEnumerable<PostalCode>> GetAllWithIncludes() => _repository.GetAllIncludingAsync(
            pc => pc.City,
            pc => pc.City.Province,
            pc => pc.City.Province.Country);

        protected override Task<PostalCode> GetWithIncludes(int id) => _repository.GetIncludingAsync(
            id, 
            pc => pc.City,
            pc => pc.City.Province,
            pc => pc.City.Province.Country);

        // get by CityId
        public async Task<IEnumerable<PostalCodeDTO>> GetByCityIdAsync(int cityId)
        {
            var postalCodes = await _repository.GetByCityIdAsync(cityId);
            return postalCodes.Select(MapToDTO);
        }

    }
}
