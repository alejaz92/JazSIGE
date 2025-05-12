using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.IVAType;
using CatalogService.Business.Models.SellCondition;
using CatalogService.Business.Models.Supplier;
using CatalogService.Business.Models.Transport;
using CatalogService.Business.Models.Warehouse;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class SupplierService : GenericService<Supplier, SupplierDTO, SupplierCreateDTO>, ISupplierService
    {
        private readonly ISupplierRepository _repository;
        private readonly IIVATypeService _ivaTypeService;
        private readonly IWarehouseService _warehouseService;
        private readonly ITransportService _transportService;
        private readonly ISellConditionService _sellConditionService;
        public SupplierService(
            ISupplierRepository repository,
            IIVATypeService ivaTypeService,
            IWarehouseService warehouseService,
            ITransportService transportService,
            ISellConditionService sellConditionService
            ) : base(repository) { 
            _repository = repository;
            _ivaTypeService = ivaTypeService;
            _warehouseService = warehouseService;
            _transportService = transportService;
            _sellConditionService = sellConditionService;
        }

        protected override SupplierDTO MapToDTO(Supplier entity)
        {
            return new SupplierDTO
            {
                Id = entity.Id,
                TaxId = entity.TaxId,
                CompanyName = entity.CompanyName,
                ContactName = entity.ContactName,
                Address = entity.Address,
                PostalCodeId = entity.PostalCodeId,
                PostalCode = entity.PostalCode.Code,
                CityId = entity.PostalCode.CityId,
                City = entity.PostalCode.City.Name,
                ProvinceId = entity.PostalCode.City.ProvinceId,
                Province = entity.PostalCode.City.Province.Name,
                CountryId = entity.PostalCode.City.Province.CountryId,
                Country = entity.PostalCode.City.Province.Country.Name,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                IVATypeId = entity.IVATypeId,
                IVAType = entity.IVAType.Description,
                WarehouseId = entity.WarehouseId,
                Warehouse = entity.Warehouse.Description,
                TransportId = entity.TransportId,
                Transport = entity.Transport.Name,
                SellConditionId = entity.SellConditionId,
                SellCondition = entity.SellCondition.Description,
                IsActive = entity.IsActive
            };
        }

        protected override Supplier MapToDomain(SupplierCreateDTO dto)
        {
            return new Supplier
            {
                TaxId = dto.TaxId,
                CompanyName = dto.CompanyName,
                ContactName = dto.ContactName,
                Address = dto.Address,
                PostalCodeId = dto.PostalCodeId,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                IVATypeId = dto.IVATypeId,
                WarehouseId = dto.WarehouseId,
                TransportId = dto.TransportId,
                SellConditionId = dto.SellConditionId
            };
        }

        protected override void UpdateDomain(Supplier entity, SupplierCreateDTO dto)
        {
            entity.TaxId = dto.TaxId;
            entity.CompanyName = dto.CompanyName;
            entity.ContactName = dto.ContactName;
            entity.Address = dto.Address;
            entity.PostalCodeId = dto.PostalCodeId;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.Email = dto.Email;
            entity.IVATypeId = dto.IVATypeId;
            entity.WarehouseId = dto.WarehouseId;
            entity.TransportId = dto.TransportId;
            entity.SellConditionId = dto.SellConditionId;
        }

        public async Task<bool> IsSupplierNameUnique(string Name)
        {
            var suppliers = await _repository.FindAsync(c => c.CompanyName == Name);
            return !suppliers.Any();
        }

        public override async Task<string?> ValidateBeforeSave(SupplierCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.CompanyName))
                return "Supplier's company name is mandatory.";
            var isUnique = await IsSupplierNameUnique(model.CompanyName);
            if (!isUnique)
                return "Supplier already exists.";
            return null;
        }

        protected override Task<IEnumerable<Supplier>> GetAllWithIncludes()
        {
            return _repository.GetAllIncludingAsync(
                s => s.PostalCode,
                s => s.PostalCode.City,
                s => s.PostalCode.City.Province,
                s => s.PostalCode.City.Province.Country,
                s => s.Transport,
                s => s.Warehouse,
                s => s.Transport,
                s => s.SellCondition,
                s => s.IVAType
            );

        }

        protected override Task<Supplier> GetWithIncludes(int id)
        {
            return _repository.GetIncludingAsync(
                id,
                s => s.PostalCode,
                s => s.PostalCode.City,
                s => s.PostalCode.City.Province,
                s => s.PostalCode.City.Province.Country,
                s => s.Transport,
                s => s.Warehouse,
                s => s.Transport,
                s => s.SellCondition,
                s => s.IVAType
            );
        }

        protected override async Task EnsureHierarchyActivationAsync(Supplier entity)
        {
            IVATypeDTO ivaType = await _ivaTypeService.GetByIdAsync( entity.IVATypeId );
            if (!ivaType.IsActive) await _ivaTypeService.UpdateStatusAsync(ivaType.Id, true);

            WarehouseDTO warehouse = await _warehouseService.GetByIdAsync(entity.WarehouseId);
            if (!warehouse.IsActive) await _warehouseService.UpdateStatusAsync(warehouse.Id, true);

            TransportDTO transport = await _transportService.GetByIdAsync(entity.TransportId);
            if(!transport.IsActive) await _transportService.UpdateStatusAsync(transport.Id, true);

            SellConditionDTO sellCondition = await _sellConditionService.GetByIdAsync(entity.SellConditionId);
            if(!sellCondition.IsActive) await _sellConditionService.UpdateStatusAsync(sellCondition.Id, true);  
        }
    }
}
