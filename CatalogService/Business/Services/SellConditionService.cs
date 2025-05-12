using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.SellCondition;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class SellConditionService : GenericService<SellCondition, SellConditionDTO, SellConditionCreateDTO>, ISellConditionService
    {
        private readonly ISellConditionRepository _repository;
        private readonly ICustomerValidatorService _customerValidatorService;
        private readonly ISupplierValidatorService _supplierValidatorService;

        public SellConditionService(
            ISellConditionRepository repository,
            ICustomerValidatorService customerValidatorService,
            ISupplierValidatorService supplierValidatorService
            ) : base(repository)
        {
            _repository = repository;
            _customerValidatorService = customerValidatorService;
            _supplierValidatorService = supplierValidatorService;
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

        protected override async Task<bool> IsInUseAsync(int id)
        {
            var activeCustomers = await _customerValidatorService.ActiveCustomersBySellCondition(id);
            if (activeCustomers > 0) return true;

            var activeSuppliers = await _supplierValidatorService.ActiveSuppliersBySellCondition(id);
            if(activeSuppliers > 0) return true;
            return false;

        }
    }
}
