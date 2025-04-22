using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.SellCondition;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class SellConditionService : GenericService<SellCondition, SellConditionDTO, SellConditionCreateDTO>, ISellConditionService
    {
        public SellConditionService(ISellConditionRepository repository) : base(repository)
        {
        }

        protected override SellConditionDTO MapToDTO(SellCondition entity)
        {
            return new SellConditionDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }
        protected override SellCondition MapToDomain(SellConditionCreateDTO dto)
        {
            return new SellCondition
            {
                Description = dto.Description
            };
        }
        protected override void UpdateDomain(SellCondition entity, SellConditionCreateDTO dto)
        {
            entity.Description = dto.Description;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        public async Task<bool> IsSellConditionDescriptionUnique(string Description)
        {
            var sellConditions = await _repository.FindAsync(b => b.Description == Description);
            return !sellConditions.Any();
        }
        public override async Task<string?> ValidateBeforeSave(SellConditionCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return "Sell Condition description is mandatory.";
            var isUnique = await IsSellConditionDescriptionUnique(model.Description);
            if (!isUnique)
                return "Sell Condition already exists.";
            return null;
        }
    }
}
