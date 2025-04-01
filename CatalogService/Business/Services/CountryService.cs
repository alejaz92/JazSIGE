using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Country;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class CountryService : GenericService<Country, CountryDTO, CountryCreateDTO>, ICountryService
    {
        public CountryService(IGenericRepository<Country> repository) : base(repository)
        {
        }

        protected override CountryDTO MapToDTO(Country entity)
        {
            return new CountryDTO
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        protected override Country MapToDomain(CountryCreateDTO createDTO)
        {
            return new Country
            {
                Name = createDTO.Name
            };
        }

        protected override void UpdateDomain(Country entity, CountryCreateDTO model)
        {
            entity.Name = model.Name;
        }

        public async Task<bool> IsCountryDescriptionUnique(string name)
        {
            var countries = await _repository.FindAsync(c => c.Name == name);
            return countries.Count() == 0;
        }

        public override async Task<string?> ValidateBeforeSave(CountryCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Name)) 
                return "Country name is required";

            var isUnique = await IsCountryDescriptionUnique(model.Name);
            if (!isUnique)
                return "Country name must be unique";
            return null;
        }
    }
}
