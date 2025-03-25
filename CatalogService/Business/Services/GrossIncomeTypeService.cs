using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.GrossIncomeType;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class GrossIncomeTypeService : GenericService<GrossIncomeType, GrossIncomeTypeDTO, GrossIncomeTypeCreateDTO>, IGrossIncomeTypeService
    {
        public GrossIncomeTypeService(IGenericRepository<GrossIncomeType> repository) : base(repository)
        {
        }

        protected override GrossIncomeTypeDTO MapToDTO(GrossIncomeType entity)
        {
            return new GrossIncomeTypeDTO
            {
                Id = entity.Id,
                Description = entity.Description
            };
        }
        protected override GrossIncomeType MapToDomain(GrossIncomeTypeCreateDTO dto)
        {
            return new GrossIncomeType
            {
                Description = dto.Description
            };
        }
        protected override void UpdateDomain(GrossIncomeType entity, GrossIncomeTypeCreateDTO dto)
        {
            entity.Description = dto.Description;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        public async Task<bool> IsGrossIncomeTypeDescriptionUnique(string Description)
        {
            var grossIncomeTypes = await _repository.FindAsync(b => b.Description == Description);
            return !grossIncomeTypes.Any();
        }
        public override async Task<string?> ValidateBeforeSave(GrossIncomeTypeCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return "Gross Income Type description is mandatory.";
            var isUnique = await IsGrossIncomeTypeDescriptionUnique(model.Description);
            if (!isUnique)
                return "Gross Income Type description already exists.";
            return null;
        }
    }
}
