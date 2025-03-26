using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Line;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class LineService : GenericService<Line, LineDTO, LineCreateDTO>, ILineService
    {
        public LineService(IGenericRepository<Line> repository) : base(repository) { }

        protected override LineDTO MapToDTO(Line entity)
        {
            return new LineDTO
            {
                Id = entity.Id,
                Description = entity.Description,
                LineGroupId = entity.LineGroupId,
                LineGroupDescription = entity.LineGroup.Description
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
    }
}
