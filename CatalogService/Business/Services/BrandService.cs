using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Brand;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class BrandService : GenericService<Brand, BrandDTO, BrandCreateDTO>, IBrandService
    {
        public BrandService(IGenericRepository<Brand> repository) : base(repository)
        {
        }

        protected override BrandDTO MapToDTO(Brand entity)
        {
            return new BrandDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        protected override Brand MapToDomain(BrandCreateDTO dto)
        {
            return new Brand
            {
                Description = dto.Description,
                IsActive = true
            };

        }

        protected override void UpdateDomain(Brand entity, BrandCreateDTO dto)
        {
            entity.Description = dto.Description;
            //entity.UpdatedAt = DateTime.UtcNow;
        }

        public async Task<bool> IsBrandDescriptionUnique(string Description)
        {
            var brands = await _repository.FindAsync(b => b.Description == Description);
            return !brands.Any();
        }

        public override async Task<string> ValidateBeforeSave(BrandCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description)) return "Brand Name is mandatory.";

            var isUnique = await IsBrandDescriptionUnique(model.Description);
            if (!isUnique) return "Brand Name already exists.";

            return null;
        }
    }
}
