using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Line;
using CatalogService.Business.Models.LineGroup;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class LineService : GenericService<Line, LineDTO, LineCreateDTO>, ILineService
    {
        private readonly ILineRepository _repository;
        private readonly IArticleValidatorService _articleValidatorService;
        private readonly ILineGroupService _lineGroupService;
        public LineService(
            ILineRepository repository,
            IArticleValidatorService articleValidatorService,
            ILineGroupService lineGroupService
        ) : base(repository) {
            _repository = repository;
            _articleValidatorService = articleValidatorService;
            _lineGroupService = lineGroupService;
        }

        protected override LineDTO MapToDTO(Line entity)
        {
            return new LineDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                LineGroupId = entity.LineGroupId,
                LineGroupDescription = entity.LineGroup.Description,
                IsActive = entity.IsActive
            };
        }
        protected override Line MapToDomain(LineCreateDTO dto)
        {
            return new Line
            {
                Description = dto.Description,
                LineGroupId = dto.LineGroupId
            };
        }
        protected override void UpdateDomain(Line entity, LineCreateDTO dto)
        {
            entity.Description = dto.Description;
            entity.LineGroupId = dto.LineGroupId;
        }
        public async Task<bool> IsLineDescriptionUnique(string Description)
        {
            var lines = await _repository.FindAsync(b => b.Description == Description);
            return !lines.Any();
        }
        public override async Task<string?> ValidateBeforeSave(LineCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return "Line description is mandatory.";
            var isUnique = await IsLineDescriptionUnique(model.Description);
            if (!isUnique)
                return "Line already exists.";
            return null;
        }

        protected override Task<IEnumerable<Line>> GetAllWithIncludes() => _repository.GetAllIncludingAsync(line => line.LineGroup);

        protected override Task<Line> GetWithIncludes(int id) => _repository.GetIncludingAsync(id, line => line.LineGroup);

        // get by lineGroupId
        public async Task<IEnumerable<LineDTO>> GetByLineGroupIdAsync(int lineGroupId)
        {
            var lines = await _repository.GetByLineGroupIdAsync(lineGroupId);
            return lines.Select(line => new LineDTO
            {
                Id = line.Id,
                Description = line.Description,
                LineGroupId = line.LineGroupId,
                LineGroupDescription = line.LineGroup.Description,
                IsActive = line.IsActive
            }).ToList();

        }

        protected override async Task<bool> IsInUseAsync(int id)
        {
            var activeArticles = await _articleValidatorService.ActiveArticlesByBrand(id);

            if (activeArticles > 0) return true;
            return false;

        }

        public async Task<int> ActiveLinesByLineGroup(int lineGroupId)
        {
            var lines = await _repository.FindAsync(
                a => a.LineGroupId == lineGroupId &&
                a.IsActive);

            return lines.Count();
        }

        protected override async Task EnsureHierarchyActivationAsync(Line entity)
        {
            // check if line group is activated
            LineGroupDTO lineGroup = await _lineGroupService.GetByIdAsync(entity.LineGroupId);

            if (!lineGroup.IsActive) 
                await _lineGroupService.UpdateStatusAsync(lineGroup.Id, true);
        }
    }
}
