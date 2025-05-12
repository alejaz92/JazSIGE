using CatalogService.Business.Interfaces;
using CatalogService.Infrastructure.Interfaces;

namespace CatalogService.Business.Services
{
    public class SupplierValidatorService : ISupplierValidatorService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierValidatorService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<int> ActiveSuppliersByIVAType(int ivaTypeId)
        {
            var suppliers = await _supplierRepository.FindAsync(s => s.IVATypeId == ivaTypeId && s.IsActive);
            return suppliers.Count();
        }

        public async Task<int> ActiveSuppliersBySellCondition(int sellConditionId)
        {
            var suppliers = await _supplierRepository.FindAsync(s => s.SellConditionId == sellConditionId && s.IsActive);
            return suppliers.Count();
        }
    }
}
