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
        private readonly IPurchaseService _purchaseService;

        public DispatchService(
            IDispatchRepository dispatchRepository,
            ICatalogServiceClient catalogServiceClient,
            IUserServiceClient userServiceClient,
            IPurchaseService purchaseService)
        {
            _dispatchRepository = dispatchRepository;
            _catalogServiceClient = catalogServiceClient;
            _userServiceClient = userServiceClient;
            _purchaseService = purchaseService;
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

        // create dispatch. purchaseId puede ser null -> despacho independiente
        public async Task<int> CreateAsync(DispatchCreateDTO dto, int userId, int? purchaseId)
        {
            // si se asocia a compra, validar existenca y que no tenga despacho ya
            if (purchaseId.HasValue)
            {
                var purchase = await _purchaseService.GetByIdAsync(purchaseId.Value);
                if (purchase == null)
                    throw new Exception("Purchase not found");

                if (purchase.Dispatch != null)
                    throw new Exception("Purchase already has a dispatch");
            }

            // check if user exists
            var user = await _userServiceClient.GetUserNameAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var dispatch = new Infrastructure.Models.Dispatch
            {
                Code = dto.Code,
                Origin = dto.Origin,
                Date = dto.Date,
                PurchaseId = purchaseId
            };
            dispatch = await _dispatchRepository.AddAsync(dispatch);
            await _dispatchRepository.SaveChangesAsync();

            return dispatch.Id;
        }
    }
}