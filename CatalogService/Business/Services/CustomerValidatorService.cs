using CatalogService.Business.Interfaces;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class CustomerValidatorService : ICustomerValidatorService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerValidatorService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<int> ActiveCustomersByIVAType(int ivaTypeId)
        {
            var customers = await _customerRepository.FindAsync(c => c.IVATypeId == ivaTypeId && c.IsActive);
            return customers.Count();
        }

        public async Task<int> ActiveCustomersBySellCondition(int sellConditionID)
        {
            var customers = await _customerRepository.FindAsync(c => c.SellConditionId == sellConditionID && c.IsActive);
            return customers.Count();
        }

        public async Task<int> ActiveCustomersByPriceList(int priceListId)
        {
            var customers = await _customerRepository.FindAsync(c => c.AssignedPriceListId == priceListId && c.IsActive);
            return customers.Count();
        }
    }
}
