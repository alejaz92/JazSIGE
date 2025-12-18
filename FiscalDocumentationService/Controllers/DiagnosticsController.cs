using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.Clients;
using FiscalDocumentationService.Business.Models.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FiscalDocumentationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DiagnosticsController : ControllerBase
    {
        private readonly ICompanyServiceClient _companyClient;
        private readonly IConfiguration _configuration;

        public DiagnosticsController(ICompanyServiceClient companyClient, IConfiguration configuration)
        {
            _companyClient = companyClient;
            _configuration = configuration;
        }

        [HttpGet("arca")]
        public async Task<ActionResult<ArcaDiagnosticsResponseDTO>> GetArcaDiagnostics()
        {
            var response = new ArcaDiagnosticsResponseDTO();

            // -------------------------------------------------
            // 1) Company fiscal settings
            // -------------------------------------------------
            var companyFiscal = await _companyClient.GetCompanyFiscalSettingsAsync();

            if (companyFiscal == null)
            {
                response.EffectiveMode = "Blocked:NoCompanyFiscalSettings";
                response.BlockingReasons.Add("Company fiscal settings not found in CompanyService.");
            }
            else
            {
                response.CompanyFiscal = new CompanyFiscalDiagnosticsDTO
                {
                    ArcaEnabled = companyFiscal.ArcaEnabled,
                    ArcaEnvironment = companyFiscal.ArcaEnvironment,
                    ArcaPointOfSale = companyFiscal.ArcaPointOfSale,
                    TaxIdMasked = MaskTaxId(companyFiscal.TaxId),
                    ArcaInvoiceTypesEnabled = string.IsNullOrWhiteSpace(companyFiscal.ArcaInvoiceTypesEnabled)
                        ? new List<string>()
                        : companyFiscal.ArcaInvoiceTypesEnabled.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .ToList()
                };

                // Gate principal (el mismo que usás en emisión)
                if (companyFiscal.ArcaEnabled && companyFiscal.ArcaPointOfSale == null)
                {
                    response.BlockingReasons.Add(
                        "ARCA is enabled but PointOfSale is not configured in CompanyService.");
                }
            }

            // -------------------------------------------------
            // 2) AppSettings (sin exponer secretos)
            // -------------------------------------------------
            var arcaEnv = _configuration["Arca:Environment"];

            var wsaaHomo = _configuration["Arca:Wsaa:HomologationUrl"];
            var wsaaProd = _configuration["Arca:Wsaa:ProductionUrl"];
            var wsfeBase = _configuration["Arca:Wsfe:BaseUrl"];

            var pfxPath = _configuration["Arca:Certificate:PfxPath"];
            var pfxPassword = _configuration["Arca:Certificate:Password"];

            response.AppSettings = new ArcaAppSettingsDiagnosticsDTO
            {
                ArcaEnvironment = arcaEnv,

                WsaaConfigured = !string.IsNullOrWhiteSpace(wsaaHomo)
                                 || !string.IsNullOrWhiteSpace(wsaaProd),

                WsfeConfigured = !string.IsNullOrWhiteSpace(wsfeBase),

                CertificatePathConfigured = !string.IsNullOrWhiteSpace(pfxPath),
                CertificateFileExists = !string.IsNullOrWhiteSpace(pfxPath)
                                        && System.IO.File.Exists(pfxPath),
                CertificatePasswordConfigured = !string.IsNullOrWhiteSpace(pfxPassword)
            };

            // -------------------------------------------------
            // 3) Effective mode
            // -------------------------------------------------
            response.EffectiveMode = ResolveEffectiveMode(companyFiscal, response);

            // -------------------------------------------------
            // 4) Warnings útiles
            // -------------------------------------------------
            if (companyFiscal != null)
            {
                // Mismatch de environment
                if (!string.IsNullOrWhiteSpace(arcaEnv)
                    && !string.Equals(arcaEnv, companyFiscal.ArcaEnvironment,
                        StringComparison.OrdinalIgnoreCase))
                {
                    response.Warnings.Add(
                        "Arca environment differs between appsettings and CompanyService.");
                }

                // Está habilitado ARCA pero no hay cert (no bloquea dummy, sí WSAA real)
                if (companyFiscal.ArcaEnabled && !response.AppSettings.CertificateFileExists)
                {
                    response.Warnings.Add(
                        "Certificate file not found. WSAA real cannot be tested yet.");
                }
            }

            return Ok(response);
        }

        private static string ResolveEffectiveMode(
            CompanyFiscalSettingsDTO? companyFiscal,
            ArcaDiagnosticsResponseDTO response)
        {
            if (companyFiscal == null)
                return "Blocked";

            if (companyFiscal.ArcaEnabled && companyFiscal.ArcaPointOfSale == null)
                return "Blocked:MissingPointOfSale";

            if (!companyFiscal.ArcaEnabled)
                return "Dummy";

            if (!response.AppSettings.CertificateFileExists)
                return "ArcaEnabled:NoCertificate";

            return "ArcaEnabled:ReadyForWsaa";
        }

        private static string MaskTaxId(string taxId)
        {
            var clean = taxId.Replace("-", "").Trim();
            if (clean.Length <= 4) return "***";

            return new string('*', clean.Length - 4) + clean[^4..];
        }
    }
}