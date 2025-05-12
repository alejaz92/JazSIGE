using CatalogService.Business.Interfaces;
using CatalogService.Infrastructure.Interfaces;

namespace CatalogService.Business.Services
{
    public class LineValidatorService : ILineValidatorService
    {
        private readonly ILineRepository _lineRepository;

        public LineValidatorService(ILineRepository lineRepository)
        {
            _lineRepository = lineRepository;
        }

        public async Task<int> ActiveLinesByLineGroup(int lineGroupId)
        {
            var lines = await _lineRepository.FindAsync(
                l => l.LineGroupId == lineGroupId && l.IsActive
            );

            return lines.Count();
        }
    }
}
