using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.BankAccount;
using CatalogService.Infrastructure.Interfaces;
using CatalogService.Infrastructure.Models;

namespace CatalogService.Business.Services
{
    public class BankAccountService : GenericService<BankAccount, BankAccountDTO, BankAccountCreateDTO>, IBankAccountService
    {
        private readonly IBankAccountRepository _repository;
        public BankAccountService(
            IBankAccountRepository repository
            ) : base(repository)
        {
            _repository = repository;
        }
        protected override BankAccountDTO MapToDTO(BankAccount entity)
        {
            return new BankAccountDTO
            {
                Id = entity.Id,
                AccountNumber = entity.AccountNumber,
                BankId = entity.BankId,
                BankName = entity.Bank != null ? entity.Bank.Name : string.Empty,
                Cbu = entity.Cbu,
                Alias = entity.Alias,
                Currency = entity.Currency,
                AccountType = entity.AccountType,
                IsActive = entity.IsActive
            };
        }
        protected override BankAccount MapToDomain(BankAccountCreateDTO dto)
        {
            return new BankAccount
            {
                AccountNumber = dto.AccountNumber,
                BankId = dto.BankId,
                Cbu = dto.Cbu,
                Alias = dto.Alias,
                Currency = dto.Currency,
                AccountType = dto.AccountType,
                IsActive = true
            };
        }
        protected override void UpdateDomain(BankAccount entity, BankAccountCreateDTO dto)
        {
            entity.AccountNumber = dto.AccountNumber;
            entity.BankId = dto.BankId;
            entity.Cbu = dto.Cbu;
            entity.Alias = dto.Alias;
            entity.Currency = dto.Currency;
            entity.AccountType = dto.AccountType;

        }
        public async Task<bool> IsAccountNumberUnique(string accountNumber)
        {
            var accounts = await _repository.FindAsync(b => b.AccountNumber == accountNumber);
            return !accounts.Any();
        }

        public override async Task<string> ValidateBeforeSave(BankAccountCreateDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.AccountNumber)) return "Account Number is mandatory.";
            if (model.BankId <= 0) return "BankId is mandatory.";
            if (string.IsNullOrWhiteSpace(model.Cbu)) return "CBU is mandatory.";
            if (string.IsNullOrWhiteSpace(model.Alias)) return "Alias is mandatory.";
            if (string.IsNullOrWhiteSpace(model.Currency)) return "Currency is mandatory.";
            if (string.IsNullOrWhiteSpace(model.AccountType)) return "Account Type is mandatory.";
            var isUnique = await IsAccountNumberUnique(model.AccountNumber);
            if (!isUnique) return "Account Number already exists.";
            return null;
        }

    }
}
