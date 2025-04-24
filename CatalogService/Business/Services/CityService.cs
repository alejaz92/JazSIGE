using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.City;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class CityService : GenericService<City, CityDTO, CityCreateDTO>, ICityService
    {
        private readonly ICityRepository _repository;
        public CityService(ICityRepository repository) : base(repository)
        {
            _repository = repository;
        }

        protected override CityDTO MapToDTO(City entity)
        {
            return new CityDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                ProvinceId = entity.ProvinceId,
                ProvinceName = entity.Province.Name
            };
        }

        protected override City MapToDomain(CityCreateDTO dto)
        {
            return new City
            {
                Name = dto.Name,
                ProvinceId = dto.ProvinceId
            };
        }

        protected override void UpdateDomain(City entity, CityCreateDTO dto)
        {
            entity.Name = dto.Name;
            entity.ProvinceId = dto.ProvinceId;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        public async Task<bool> IsCityDescriptionUnique(string Name)
        {
            var cities = await _repository.FindAsync(b => b.Name == Name);
            return !cities.Any();
        }
        public override async Task<string?> ValidateBeforeSave(CityCreateDTO model)
        {

            return null;
        }

        protected override Task<IEnumerable<City>> GetAllWithIncludes() => _repository.GetAllIncludingAsync(c => c.Province);

        protected override Task<City> GetWithIncludes(int id) => _repository.GetIncludingAsync(id, c => c.Province);

        // get by ProvinceId
        public async Task<IEnumerable<CityDTO>> GetByProvinceIdAsync(int provinceId)
        {
            var cities = await _repository.GetByProvinceIdAsync(provinceId);
            return cities.Select(MapToDTO);
        }
    }
}
