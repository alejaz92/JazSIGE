using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.LineGroup;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class LineGroupService : GenericService<LineGroup, LineGroupDTO, LineGroupCreateDTO>, ILineGroupService
    {
        private readonly ILineService _lineService;
        private readonly ILineGroupService _lineGroupService;
        public LineGroupService(
            ILineGroupRepository repository,
            ILineService lineService
            ) : base(repository) 
        { 
            _lineService = lineService;
        }

        protected override LineGroupDTO MapToDTO(LineGroup entity)
        {
            return new LineGroupDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        protected override LineGroup MapToDomain(LineGroupCreateDTO dto)
        {
            return new LineGroup
            {
                Description = dto.Description,
            };
        }

        protected override void UpdateDomain(LineGroup entity, LineGroupCreateDTO dto)
        {
            entity.Description = dto.Description;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        public async Task<bool> IsLineGroupDescriptionUnique(string Description)
        {
            var lineGroups = await _repository.FindAsync(b => b.Description == Description);
            return !lineGroups.Any();
        }

        public override async Task<string?> ValidateBeforeSave(LineGroupCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return "Line Group description is mandatory.";
            var isUnique = await IsLineGroupDescriptionUnique(model.Description);
            if (!isUnique)
                return "Line group already exists.";
            return null;
        }

        protected override async Task<bool> IsInUseAsync(int id)
        {
            var activeLines = await _lineService.ActiveLinesByLineGroup(id);

            if (activeLines > 0) return true;
            return false;
        }
    }
}
