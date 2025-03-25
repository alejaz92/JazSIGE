using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Province;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class ProvinceService : GenericService<Province, ProvinceDTO, ProvinceCreateDTO>, IProvinceService
    {
        public ProvinceService(IGenericRepository<Province> repository) : base(repository)
        {
        }

        protected override ProvinceDTO MapToDTO(Province entity)
        {
            return new ProvinceDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                CountryId = entity.CountryId,
                CountryName = entity.Country.Name
            };
        }

        protected override Province MapToDomain(ProvinceCreateDTO dto)
        {
            return new Province
            {
                Name = dto.Name,
                CountryId = dto.CountryId
            };
        }

        protected override void UpdateDomain(Province entity, ProvinceCreateDTO dto)
        {
            entity.Name = dto.Name;
            entity.CountryId = dto.CountryId;
        }

        public override async Task<string?> ValidateBeforeSave(ProvinceCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return "Name is required";
            }
            if (model.CountryId <= 0)
            {
                return "Country is required";
            }
            return null;
        }

        protected override Task<IEnumerable<Province>> GetAllWithIncludes() => _repository.GetAllIncludingAsync(p => p.Country);

        protected override Task<Province> GetWithIncludes(int id) => _repository.GetIncludingAsync(id, p => p.Country);
    }
}
