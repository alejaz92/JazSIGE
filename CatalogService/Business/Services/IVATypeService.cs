using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.IVAType;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class IVATypeService : GenericService<IVAType, IVATypeDTO, IVATypeCreateDTO>, IIVATypeService
    {
        private readonly IIVATypeRepository _repository;
        private readonly ICustomerValidatorService _customerValidatorService;
        private readonly ISupplierValidatorService _supplierValidatorService;
        public IVATypeService(
            IIVATypeRepository repository,
            ICustomerValidatorService customerValidatorService,
            ISupplierValidatorService supplierValidatorService
            ) : base(repository)
        {
            _repository = repository;
            _customerValidatorService = customerValidatorService;
            _supplierValidatorService = supplierValidatorService;
        }

        protected  override IVATypeDTO MapToDTO(IVAType entity)
        {
            return new IVATypeDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        protected override IVAType MapToDomain(IVATypeCreateDTO dto)
        {
            return new IVAType
            {
                Description = dto.Description
            };
        }

        protected override void UpdateDomain(IVAType entity, IVATypeCreateDTO dto)
        {
            entity.Description = dto.Description;
        }

        public async Task<bool> IsIVATypeDescriptionUnique(string Description)
        {
            var ivatypes = await _repository.FindAsync(b => b.Description == Description);
            return !ivatypes.Any();
        }

        public override async Task<string?> ValidateBeforeSave(IVATypeCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return "IVA Type description is mandatory.";
            var isUnique = await IsIVATypeDescriptionUnique(model.Description);
            if (!isUnique)
                return "IVA Type description already exists";
            return null;
        }

        protected override async Task<bool> IsInUseAsync(int id)
        {
            var activeCustomers = await _customerValidatorService.ActiveCustomersByIVAType(id);
            if (activeCustomers > 0) return true;

            var activeSuppliers = await _supplierValidatorService.ActiveSuppliersByIVAType(id);
            if(activeSuppliers > 0) return true;
            return false;
        }
    }
}
