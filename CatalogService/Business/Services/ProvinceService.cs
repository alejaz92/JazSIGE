using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Province;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class ProvinceService : GenericService<Province, ProvinceDTO, ProvinceCreateDTO>, IProvinceService
    {
        private readonly IProvinceRepository _repository;
        public ProvinceService(IProvinceRepository repository) : base(repository)
        {
            _repository = repository;
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


        // get by CountryId
        public async Task<IEnumerable<ProvinceDTO>> GetByCountryIdAsync(int countryId)
        {
            var provinces = await _repository.GetByCountryIdAsync(countryId);
            return provinces.Select(MapToDTO);
        }
    }
}
