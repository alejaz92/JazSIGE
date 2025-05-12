using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.PriceList;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class PriceListService : GenericService<PriceList, PriceListDTO, PriceListCreateDTO>, IPriceListService
    {
        private readonly IPriceListRepository _repository;
        private readonly ICustomerValidatorService _customerValidatorService;
        public PriceListService(
            IPriceListRepository repository,
            ICustomerValidatorService customerValidatorService
            ) : base(repository)
        {
            _repository = repository;
            _customerValidatorService = customerValidatorService;
        }
        protected override PriceListDTO MapToDTO(PriceList entity)
        {
            return new PriceListDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                IsActive = entity.IsActive,
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

        protected override async Task<bool> IsInUseAsync(int id)
        {
            var activeCustomers = await _customerValidatorService.ActiveCustomersByPriceList(id);

            if (activeCustomers > 0) return true;
            return false;
        }

    }
}
