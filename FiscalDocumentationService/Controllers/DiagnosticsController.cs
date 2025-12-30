using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.Clients;
using FiscalDocumentationService.Business.Models.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace FiscalDocumentationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DiagnosticsController : ControllerBase
    {
        //private readonly ICompanyServiceClient _companyClient;
        private readonly IConfiguration _configuration;
        private readonly IArcaAuthClient _arcaAuth;
        private readonly IArcaWsfeClient _arcaWsfeClient;
        private readonly ICompanyServiceClient _companyServiceClient;

        public DiagnosticsController(
            //ICompanyServiceClient companyClient, 
            IConfiguration configuration, 
            IArcaAuthClient arcaAuth,
            IArcaWsfeClient arcaWsfeClient,
            ICompanyServiceClient companyServiceClient)
        {
            //_companyClient = companyClient;
            _configuration = configuration;
            _arcaAuth = arcaAuth;
            _arcaWsfeClient = arcaWsfeClient;
            _companyServiceClient = companyServiceClient;
        }


        [HttpGet("wsaa")]
        public async Task<IActionResult> TestWsaa()
        {
            var ticket = await _arcaAuth.GetAccessTicketAsync("wsfe");

            return Ok(new
            {
                tokenLength = ticket.Token.Length,
                signLength = ticket.Sign.Length,
                expiresAtUtc = ticket.ExpirationTimeUtc
            });
        }

        [HttpGet("wsfe/dummy")]
        public async Task<IActionResult> WsfeDummy()
        {
            var r = await _arcaWsfeClient.DummyAsync();
            return Ok(r);
        }

        [HttpGet("wsfe/param/tipos-cbte")]
        public async Task<IActionResult> WsfeParamGetTiposCbte()
        {
            var fiscal = await _companyServiceClient.GetCompanyFiscalSettingsAsync();
            if (fiscal == null || string.IsNullOrWhiteSpace(fiscal.TaxId))
                return BadRequest("Company fiscal settings not found or missing TaxId.");

            // TaxId may come formatted (e.g., 20-xxxxxxxx-x). Keep only digits.
            var digits = Regex.Replace(fiscal.TaxId, @"\D", "");
            if (!long.TryParse(digits, out var cuit))
                return BadRequest($"Invalid TaxId format: '{fiscal.TaxId}'");

            var list = await _arcaWsfeClient.GetInvoiceTypesAsync(cuit);

            // Return ordered for readability
            return Ok(list.OrderBy(x => x.Id));
        }

        [HttpGet("wsfe/comp/ultimo-autorizado")]
        public async Task<IActionResult> WsfeGetLastAuthorized([FromQuery] int cbteType)
        {
            if (cbteType <= 0)
                return BadRequest("cbteType must be a positive integer (e.g., 1=Factura A, 6=Factura B, 11=Factura C).");

            var fiscal = await _companyServiceClient.GetCompanyFiscalSettingsAsync();
            if (fiscal == null || string.IsNullOrWhiteSpace(fiscal.TaxId))
                return BadRequest("Company fiscal settings not found or missing TaxId.");

            if (fiscal.ArcaPointOfSale == null)
                return BadRequest("Company fiscal settings missing ArcaPointOfSale.");

            var digits = new string(fiscal.TaxId.Where(char.IsDigit).ToArray());
            if (!long.TryParse(digits, out var issuerCuit))
                return BadRequest($"Invalid TaxId format: '{fiscal.TaxId}'");

            var last = await _arcaWsfeClient.GetLastAuthorizedAsync(
                issuerCuit,
                fiscal.ArcaPointOfSale.Value,
                cbteType);

            return Ok(new
            {
                issuerCuit,
                pointOfSale = fiscal.ArcaPointOfSale.Value,
                cbteType,
                lastAuthorizedNumber = last,
                nextNumber = last + 1
            });
        }
    }
}