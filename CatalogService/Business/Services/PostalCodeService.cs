using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.PostalCode;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class PostalCodeService : GenericService<PostalCode, PostalCodeDTO, PostalCodeCreateDTO>, IPostalCodeService
    {
        public PostalCodeService(IGenericRepository<PostalCode> repository) : base(repository)
        {
        }

        protected override PostalCodeDTO MapToDTO(PostalCode entity)
        {
            return new PostalCodeDTO
            {
                Id = entity.Id,
                Code = entity.Code,
                CityId = entity.CityId,
                CityName = entity.City.Name
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

        protected override Task<IEnumerable<PostalCode>> GetAllWithIncludes() => _repository.GetAllIncludingAsync(pc => pc.City);

        protected override Task<PostalCode> GetWithIncludes(int id) => _repository.GetIncludingAsync(id, pc => pc.City);

    }
}
