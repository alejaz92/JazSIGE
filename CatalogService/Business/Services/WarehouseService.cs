using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Warehouse;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class WarehouseService : GenericService<Warehouse, WarehouseDTO, WarehouseCreateDTO>, IWarehouseService   {
        public WarehouseService(IWarehouseRepository repository) : base(repository)
        {
        }

        protected override WarehouseDTO MapToDTO(Warehouse entity)
        {
            return new WarehouseDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }
        protected override Warehouse MapToDomain(WarehouseCreateDTO dto)
        {
            return new Warehouse
            {
                Description = dto.Description
            };
        }
        protected override void UpdateDomain(Warehouse entity, WarehouseCreateDTO dto)
        {
            entity.Description = dto.Description;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        public async Task<bool> IsWarehouseDescriptionUnique(string Description)
        {
            var warehouses = await _repository.FindAsync(b => b.Description == Description);
            return !warehouses.Any();
        }
        public override async Task<string?> ValidateBeforeSave(WarehouseCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return "Warehouse description is mandatory.";
            var isUnique = await IsWarehouseDescriptionUnique(model.Description);
            if (!isUnique)
                return "Warehouse already exists.";
            return null;
        }
    }
}
