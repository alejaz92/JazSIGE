using CompanyService.Business.Interfaces;
using CompanyService.Business.Models;
using CompanyService.Business.Exceptions;
using CompanyService.Infrastructure.Interfaces;

namespace CompanyService.Business.Services
{
    /// <summary>
    /// Service for managing company information business logic
    /// Handles company data retrieval, updates, and fiscal settings
    /// </summary>
    public class CompanyInfoService : ICompanyInfoService
    {
        private readonly ICatalogServiceClient _catalogClient;
        private readonly ICompanyInfoRepository _repository;

        /// <summary>
        /// Initializes a new instance of the CompanyInfoService
        /// </summary>
        /// <param name="catalogClient">Client for accessing catalog service</param>
        /// <param name="repository">Repository for company data access</param>
        public CompanyInfoService(ICatalogServiceClient catalogClient, ICompanyInfoRepository repository)
        {
            _catalogClient = catalogClient;
            _repository = repository;
        }

        /// <summary>
        /// Retrieves the complete company information
        /// </summary>
        /// <returns>Company information DTO, or null if not found</returns>
        public async Task<CompanyInfoDTO> GetAsync()
        {
            var company = await _repository.GetAsync();
            if (company == null) return null;

            // Map entity to DTO
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

        /// <summary>
        /// Updates company information
        /// Validates postal code and IVA type exist in catalog service before updating
        /// </summary>
        /// <param name="dto">Company information update DTO</param>
        /// <exception cref="ArgumentNullException">Thrown when dto is null</exception>
        /// <exception cref="NotFoundException">Thrown when company, postal code, or IVA type not found</exception>
        public async Task UpdateAsync(CompanyInfoUpdateDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Validate postal code exists in catalog service
            var postalCode = await _catalogClient.GetPostalCodeByIdAsync(dto.PostalCodeId);
            if (postalCode == null)
                throw new NotFoundException($"Postal code with ID {dto.PostalCodeId} not found");

            // Validate IVA type exists in catalog service
            var ivaType = await _catalogClient.GetIVATypeByIdAsync(dto.IVATypeId);
            if (ivaType == null)
                throw new NotFoundException($"IVA type with ID {dto.IVATypeId} not found");

            // Get existing company entity
            var company = await _repository.GetAsync();
            if (company == null)
                throw new NotFoundException("Company info not found");

            // Update company properties
            company.Name = dto.Name;
            company.ShortName = dto.ShortName;
            company.TaxId = dto.TaxId;
            company.Address = dto.Address;
            company.PostalCodeId = dto.PostalCodeId;
            // Update address details from postal code catalog data
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

        /// <summary>
        /// Updates only the company logo URL
        /// </summary>
        /// <param name="dto">Logo URL update DTO</param>
        /// <exception cref="ArgumentNullException">Thrown when dto is null</exception>
        /// <exception cref="NotFoundException">Thrown when company info not found</exception>
        public async Task UpdateLogoUrlAsync(CompanyLogoUpdateDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var company = await _repository.GetAsync();
            if (company == null)
                throw new NotFoundException("Company info not found");

            company.LogoUrl = dto.LogoUrl;
            company.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(company);
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves fiscal settings for the company (ARCA configuration)
        /// Used for invoicing system configuration
        /// </summary>
        /// <returns>Fiscal settings DTO containing ARCA configuration, or null if company not found</returns>
        public async Task<CompanyFiscalSettingsDTO?> GetFiscalSettingsAsync()
        {
            var company = await _repository.GetAsync();
            if (company == null) return null;

            // Map ArcaEnvironment enum to string representation
            string env = company.ArcaEnvironment switch
            {
                Infrastructure.Models.ArcaEnvironment.Homologation => "Homologation",
                Infrastructure.Models.ArcaEnvironment.Production => "Production",
                _ => "Production" // Default to Production for safety
            };

            return new CompanyFiscalSettingsDTO
            {
                TaxId = company.TaxId,
                ArcaEnabled = company.ArcaEnabled,
                ArcaEnvironment = env,
                ArcaPointOfSale = company.ArcaPointOfSale,
                ArcaInvoiceTypesEnabled = company.ArcaInvoiceTypesEnabled
            };
        }
    }
}
