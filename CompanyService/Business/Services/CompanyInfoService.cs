using CompanyService.Business.Interfaces;
using CompanyService.Business.Models;
using CompanyService.Infrastructure.Interfaces;

namespace CompanyService.Business.Services
{
    public class CompanyInfoService : ICompanyInfoService
    {
        private readonly ICatalogServiceClient _catalogClient;
        private readonly ICompanyInfoRepository _repository;

        public CompanyInfoService(ICatalogServiceClient catalogClient, ICompanyInfoRepository repository)
        {
            _catalogClient = catalogClient;
            _repository = repository;
        }

        public async Task<CompanyInfoDTO> GetAsync()
        {
            var company = await _repository.GetAsync();
            if (company == null)   return null;



            return new CompanyInfoDTO
            {
                Name = company.Name,
                ShortName = company.ShortName,
                TaxId = company.TaxId,
                Address = company.Address,
                PostalCodeId = company.PostalCodeId,
                PostalCode = company.PostalCode,
                City = company.City,
                Province = company.Province,
                Country = company.Country,
                Phone = company.Phone,
                Email = company.Email,
                LogoUrl = company.LogoUrl,
                IVATypeId = company.IVATypeId,
                IVAType = company.IVAType,
                GrossIncome = company.GrossIncome,
                DateOfIncorporation = company.DateOfIncorporation
            };
        }

        public async Task UpdateAsync(CompanyInfoUpdateDTO dto)
        {
            var postalCode = await _catalogClient.GetPostalCodeByIdAsync(dto.PostalCodeId)
                ?? throw new Exception("Postal code not found");

            var ivaType = await _catalogClient.GetIVATypeByIdAsync(dto.IVATypeId)
                ?? throw new Exception("IVA type not found");

            var company = await _repository.GetAsync();
            if (company == null) throw new Exception("Company info not found");

            company.Name = dto.Name;
            company.ShortName = dto.ShortName;
            company.TaxId = dto.TaxId;
            company.Address = dto.Address;
            company.PostalCodeId = dto.PostalCodeId;
            company.PostalCode = postalCode.Description;
            company.City = postalCode.City.Description;
            company.Province = postalCode.City.Province.Description;
            company.Country = postalCode.City.Province.Country.Description;
            company.Phone = dto.Phone;
            company.Email = dto.Email;
            company.LogoUrl = dto.LogoUrl;
            company.IVATypeId = dto.IVATypeId;
            company.IVAType = ivaType.Description;
            company.GrossIncome = dto.GrossIncome;
            company.DateOfIncorporation = dto.DateOfIncorporation;
            company.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(company);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateLogoUrlAsync(CompanyLogoUpdateDTO dto)
        {
            var company = await _repository.GetAsync();
            if (company == null)
                throw new Exception("Company info not found.");

            company.LogoUrl = dto.LogoUrl;
            company.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(company);
            await _repository.SaveChangesAsync();
        }

    }
}
