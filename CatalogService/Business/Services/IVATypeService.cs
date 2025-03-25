using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.IVAType;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class IVATypeService : GenericService<IVAType, IVATypeDTO, IVATypeCreateDTO>, IIVATypeService
    {
        public IVATypeService(IGenericRepository<IVAType> repository) : base(repository)
        {
        }

        protected  override IVATypeDTO MapToDTO(IVAType entity)
        {
            return new IVATypeDTO
            {
                Id = entity.Id,
                Description = entity.Description
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
    }
}
