using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Bank;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class BankService : GenericService<Bank, BankDTO, BankCreateDTO>, IBankService
    {
        private readonly IBankRepositoty _repository;
        public BankService(
            IBankRepositoty repository
            ) : base(repository)
        {
            _repository = repository;
        }

        protected override BankDTO MapToDTO(Bank entity)
        {
            return new BankDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code
            };
        }

        protected override Bank MapToDomain(BankCreateDTO dto)
        {
            return new Bank
            {
                Name = dto.Name,
                Code = dto.Code
            };
        }

        protected override void UpdateDomain(Bank entity, BankCreateDTO dto)
        {
            entity.Name = dto.Name;
            entity.Code = dto.Code;
        }

        public async Task<bool> IsBankNameUnique(string Name)
        {
            var banks = await _repository.FindAsync(b => b.Name == Name);
            return !banks.Any();
        }

        public override async Task<string> ValidateBeforeSave(BankCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Name)) return "Bank Name is mandatory.";
            var isUnique = await IsBankNameUnique(model.Name);
            if (!isUnique) return "Bank Name already exists.";
            
            if(model.Code <= 0) return "Bank Code must be greater than zero.";
            return string.Empty;
        }
    }
}
