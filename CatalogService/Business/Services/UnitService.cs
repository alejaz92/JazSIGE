using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Unit;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class UnitService : GenericService<Unit, UnitDTO, UnitCreateDTO>, IUnitService
    {
        public UnitService(IUnitRepository repository) : base(repository)
        {
        }

        protected override UnitDTO MapToDTO(Unit entity)
        {
            return new UnitDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                IsActive = entity.IsActive,
            };
        }
        protected override Unit MapToDomain(UnitCreateDTO dto)
        {
            return new Unit
            {
                Description = dto.Description
            };
        }
        protected override void UpdateDomain(Unit entity, UnitCreateDTO dto)
        {
            entity.Description = dto.Description;
        }
        public async Task<bool> IsUnitDescriptionUnique(string Description)
        {
            var units = await _repository.FindAsync(b => b.Description == Description);
            return !units.Any();
        }
        public override async Task<string?> ValidateBeforeSave(UnitCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return "Unit description is mandatory.";
            var isUnique = await IsUnitDescriptionUnique(model.Description);
            if (!isUnique)
                return "Unit already exists.";
            return null;
        }
    }
}
