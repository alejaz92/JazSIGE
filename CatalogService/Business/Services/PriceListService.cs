using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.PriceList;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class PriceListService : GenericService<PriceList, PriceListDTO, PriceListCreateDTO>, IPriceListService
    {
        public PriceListService(IPriceListRepository repository) : base(repository)
        {
        }
        protected override PriceListDTO MapToDTO(PriceList entity)
        {
            return new PriceListDTO
            {
                Id = entity.Id,
                Description = entity.Description
            };
        }
        protected override PriceList MapToDomain(PriceListCreateDTO dto)
        {
            return new PriceList
            {
                Description = dto.Description
            };
        }
        protected override void UpdateDomain(PriceList entity, PriceListCreateDTO dto)
        {
            entity.Description = dto.Description;
        }
        public async Task<bool> IsPriceListDescriptionUnique(string Description)
        {
            var priceLists = await _repository.FindAsync(b => b.Description == Description);
            return !priceLists.Any();
        }
        public override async Task<string?> ValidateBeforeSave(PriceListCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return "Price List description is mandatory.";
            var isUnique = await IsPriceListDescriptionUnique(model.Description);
            if (!isUnique)
                return "Price List already exists.";
            return null;
        }

    }
}
