using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;
using PurchaseService.Infrastructure.Interfaces;

namespace PurchaseService.Business.Services
{
    public class DispatchService : IDispatchService
    {
        private readonly IDispatchRepository _dispatchRepository;
        private readonly ICatalogServiceClient _catalogServiceClient;
        private readonly IUserServiceClient _userServiceClient;

        public DispatchService(IDispatchRepository dispatchRepository, ICatalogServiceClient catalogServiceClient, IUserServiceClient userServiceClient)
        {
            _dispatchRepository = dispatchRepository;
            _catalogServiceClient = catalogServiceClient;
            _userServiceClient = userServiceClient;
        }

        public async Task<IEnumerable<DispatchDTO>> GetAllAsync()
        {
            var dispatches = await _dispatchRepository.GetAllAsync();
            var list = new List<DispatchDTO>();
            foreach (var d in dispatches)
                list.Add(await MapToDTOAsync(d));

            return list;
        }

        public async Task<(IEnumerable<DispatchDTO> Items, int TotalCount)> GetAllAsync(int page, int pageSize)
        {
            var dispatches = await _dispatchRepository.GetAllAsync(page, pageSize);
            var totalCount = await _dispatchRepository.GetTotalCountAsync();
            var list = new List<DispatchDTO>();
            foreach (var d in dispatches)
                list.Add(await MapToDTOAsync(d));

            return (list, totalCount);
        }

        public async Task<DispatchDTO?> GetByIdAsync(int id)
        {
            var dispatch = await _dispatchRepository.GetByIdAsync(id);
            return dispatch == null ? null : await MapToDTOAsync(dispatch);
        }

        private async Task<DispatchDTO> MapToDTOAsync(Infrastructure.Models.Dispatch dispatch)
        {

            return new DispatchDTO
            {
                Id = dispatch.Id,
                Code = dispatch.Code,
                Origin = dispatch.Origin,
                Date = dispatch.Date,
                PurchaseId = dispatch.PurchaseId
            };
        }

    }
}
