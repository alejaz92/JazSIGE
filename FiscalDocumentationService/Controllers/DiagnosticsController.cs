using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.Arca;
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

        [HttpPost("wsfe/cae/hardcoded")]
        public async Task<IActionResult> WsfeCaeHardcoded()
        {
            var fiscal = await _companyServiceClient.GetCompanyFiscalSettingsAsync();
            if (fiscal == null || string.IsNullOrWhiteSpace(fiscal.TaxId))
                return BadRequest("Company fiscal settings not found or missing TaxId.");

            if (fiscal.ArcaPointOfSale == null)
                return BadRequest("Company fiscal settings missing ArcaPointOfSale.");

            var digits = new string(fiscal.TaxId.Where(char.IsDigit).ToArray());
            if (!long.TryParse(digits, out var issuerCuit))
                return BadRequest($"Invalid TaxId format: '{fiscal.TaxId}'");

            // HARD CODED document (start with Factura B)
            const int cbteType = 6;      // Factura B
            const int concept = 1;       // Products
            const int docType = 99;      // Consumidor Final
            const long docNumber = 0;
            const string currencyId = "PES";
            const decimal currencyRate = 1.00m;
            const int receiverVatConditionId = 5; // Consumidor Final

            // HARD CODED amounts (simple)
            var total = 1000.00m;

            var net = decimal.Round(total / 1.21m, 2);
            var vat = total - net;

            // IVA 21%: el ID suele ser 5, pero lo ideal es confirmarlo con FEParamGetTiposIva.
            // Para destrabar rápido, probamos con 5:
            const int vatId21 = 5;


            var last = await _arcaWsfeClient.GetLastAuthorizedAsync(issuerCuit, fiscal.ArcaPointOfSale.Value, cbteType);
            var next = last + 1;

            var req = new WsfeCaeRequest(
                 PointOfSale: fiscal.ArcaPointOfSale.Value,
                 CbteType: cbteType,
                 Concept: concept,
                 DocType: docType,
                 DocNumber: docNumber,
                 CbteDate: DateOnly.FromDateTime(DateTime.Now),
                 CbteFrom: next,
                 CbteTo: next,
                 CurrencyId: currencyId,
                 CurrencyRate: currencyRate,
                 TotalAmount: total,
                 NetAmount: net,
                 VatAmount: vat,
                 NotTaxedAmount: 0m,
                 ExemptAmount: 0m,
                 OtherTaxesAmount: 0m,
                 VatItems: new List<WsfeVatItem>
                 {
                    new WsfeVatItem(vatId21, net, vat)
                 },
                 ReceiverVatConditionId: receiverVatConditionId
             );

            var resp = await _arcaWsfeClient.RequestCaeAsync(issuerCuit, req);

            if (resp.Result == "R" && resp.Errors.Count == 0)
            {
                return Ok(new
                {
                    result = resp.Result,
                    message = "Rejected but no parsed errors. Check SOAP parsing or return raw SOAP for debugging.",
                    events = resp.Events
                });
            }


            return Ok(new
            {
                issuerCuit,
                req.PointOfSale,
                req.CbteType,
                cbteNumber = next,
                result = resp.Result,
                cae = resp.Cae,
                caeDueDate = resp.CaeDueDate,
                events = resp.Events,
                errors = resp.Errors
            });
        }

    }
}