using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace FiscalDocumentationService.Business.Services.Clients
{
    public class ArcaWsfeClient : IArcaWsfeClient
    {
        private readonly HttpClient _http;
        private readonly IOptions<ArcaOptions> _arcaOptions;
        private readonly IArcaAuthClient _arcaAuthClient;

        public ArcaWsfeClient(HttpClient http, IOptions<ArcaOptions> arcaOptions, IArcaAuthClient arcaAuthClient)
        {
            _http = http;
            _arcaOptions = arcaOptions;
            _arcaAuthClient = arcaAuthClient;
        }


        public async Task<WsfeDummyResult> DummyAsync(CancellationToken ct = default)
        {
            var url = ResolveWsfeUrl();

            var soapEnvelope =
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
  <soapenv:Header/>
  <soapenv:Body>
    <ar:FEDummy/>
  </soapenv:Body>
</soapenv:Envelope>";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            req.Headers.Add("SOAPAction", "\"http://ar.gov.afip.dif.FEV1/FEDummy\"");

            using var resp = await _http.SendAsync(req, ct);
            var xml = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"WSFE FEDummy error {(int)resp.StatusCode}: {xml}");

            return ParseDummyResponse(xml);
        }

        public async Task<IReadOnlyList<WsfeCbteTypeItem>> GetInvoiceTypesAsync(long issuerCuit, CancellationToken ct = default)
        {
            var url = ResolveWsfeUrl();

            var auth = await BuildAuthAsync(issuerCuit, ct);

            var soapEnvelope =
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
  <soapenv:Header/>
  <soapenv:Body>
    <ar:FEParamGetTiposCbte>
      <ar:Auth>
        <ar:Token>{SecurityElementEscape(auth.Token)}</ar:Token>
        <ar:Sign>{SecurityElementEscape(auth.Sign)}</ar:Sign>
        <ar:Cuit>{auth.Cuit}</ar:Cuit>
      </ar:Auth>
    </ar:FEParamGetTiposCbte>
  </soapenv:Body>
</soapenv:Envelope>";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            req.Headers.Add("SOAPAction", "\"http://ar.gov.afip.dif.FEV1/FEParamGetTiposCbte\"");

            using var resp = await _http.SendAsync(req, ct);
            var xml = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"WSFE FEParamGetTiposCbte error {(int)resp.StatusCode}: {xml}");

            return ParseInvoiceTypes(xml);
        }

        public async Task<long> GetLastAuthorizedAsync(long issuerCuit, int pointOfSale, int cbteType, CancellationToken ct = default)
        {
            var url = ResolveWsfeUrl();
            var auth = await BuildAuthAsync(issuerCuit, ct);
            var soapEnvelope =
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
  <soapenv:Header/>
  <soapenv:Body>
    <ar:FECompUltimoAutorizado>
      <ar:Auth>
        <ar:Token>{SecurityElementEscape(auth.Token)}</ar:Token>
        <ar:Sign>{SecurityElementEscape(auth.Sign)}</ar:Sign>
        <ar:Cuit>{auth.Cuit}</ar:Cuit>
      </ar:Auth>
      <ar:PtoVta>{pointOfSale}</ar:PtoVta>
      <ar:CbteTipo>{cbteType}</ar:CbteTipo>
    </ar:FECompUltimoAutorizado>
  </soapenv:Body>
</soapenv:Envelope>";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            req.Headers.Add("SOAPAction", "\"http://ar.gov.afip.dif.FEV1/FECompUltimoAutorizado\"");

            using var resp = await _http.SendAsync(req, ct);
            var xml = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"WSFE FECompUltimoAutorizado error {(int)resp.StatusCode}: {xml}");

            return ParseLastAuthorized(xml);

        }
            

        // -------------------------
        // Helpers
        // -------------------------

        private string ResolveWsfeUrl()
        {
            var options = _arcaOptions.Value;

            var env = (options.Environment ?? "Homologation").Trim();
            var isProd = env.Equals("Production", StringComparison.OrdinalIgnoreCase);

            // Guard rail global (aunque Dummy no emite, mantenemos coherencia)
            if (isProd && !options.AllowProductionEmission)
                throw new Exception("ARCA Production is blocked by configuration (Arca:AllowProductionEmission=false).");

            return isProd ? options.Wsfe.ProductionUrl : options.Wsfe.HomologationUrl;
        }

        private static WsfeDummyResult ParseDummyResponse(string soapXml)
        {
            var x = XDocument.Parse(soapXml);

            var resultNode = x.Descendants().FirstOrDefault(e => e.Name.LocalName == "FEDummyResult");
            if (resultNode == null)
                throw new Exception("WSFE response does not contain FEDummyResult.");

            string Read(string name) =>
                resultNode.Descendants().FirstOrDefault(e => e.Name.LocalName == name)?.Value?.Trim() ?? "";

            return new WsfeDummyResult
            {
                AppServer = Read("AppServer"),
                DbServer = Read("DbServer"),
                AuthServer = Read("AuthServer")
            };
        }

        private async Task<WsfeAuth> BuildAuthAsync(long issuerCuit, CancellationToken ct)
        {
            // WSAA already handles caching internally
            var ticket = await _arcaAuthClient.GetAccessTicketAsync("wsfe");
            return new WsfeAuth(ticket.Token, ticket.Sign, issuerCuit);
        }

        private static IReadOnlyList<WsfeCbteTypeItem> ParseInvoiceTypes(string soapXml)
        {
            var x = XDocument.Parse(soapXml);

            // The response contains: FEParamGetTiposCbteResult -> ResultGet -> CbteTipo (list)
            var result = x.Descendants().FirstOrDefault(e => e.Name.LocalName == "FEParamGetTiposCbteResult");
            if (result == null)
                throw new Exception("WSFE response does not contain FEParamGetTiposCbteResult.");

            // Optional: check Errors in response
            var errorsNode = result.Descendants().FirstOrDefault(e => e.Name.LocalName == "Errors");
            if (errorsNode != null && errorsNode.Descendants().Any(e => e.Name.LocalName == "Err"))
            {
                var firstErr = errorsNode.Descendants().First(e => e.Name.LocalName == "Err");
                var code = firstErr.Descendants().FirstOrDefault(e => e.Name.LocalName == "Code")?.Value ?? "";
                var msg = firstErr.Descendants().FirstOrDefault(e => e.Name.LocalName == "Msg")?.Value ?? "";
                throw new Exception($"WSFE returned Errors. Code={code} Msg={msg}");
            }

            var items = new List<WsfeCbteTypeItem>();

            var cbteTipoNodes = result.Descendants().Where(e => e.Name.LocalName == "CbteTipo");
            foreach (var n in cbteTipoNodes)
            {
                var idText = n.Descendants().FirstOrDefault(e => e.Name.LocalName == "Id")?.Value?.Trim();
                var desc = n.Descendants().FirstOrDefault(e => e.Name.LocalName == "Desc")?.Value?.Trim() ?? "";

                if (int.TryParse(idText, out var id))
                    items.Add(new WsfeCbteTypeItem { Id = id, Description = desc });
            }

            return items;
        }

        private static string SecurityElementEscape(string value)
        {
            // Minimal XML escaping (token/sign are base64 but keep safe)
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private readonly record struct WsfeAuth(string Token, string Sign, long Cuit);

        private static long ParseLastAuthorized(string soapXml)
        {
            var x = XDocument.Parse(soapXml);

            var result = x.Descendants().FirstOrDefault(e => e.Name.LocalName == "FECompUltimoAutorizadoResult");
            if (result == null)
                throw new Exception("WSFE response does not contain FECompUltimoAutorizadoResult.");

            // Check Errors
            var errorsNode = result.Descendants().FirstOrDefault(e => e.Name.LocalName == "Errors");
            if (errorsNode != null && errorsNode.Descendants().Any(e => e.Name.LocalName == "Err"))
            {
                var firstErr = errorsNode.Descendants().First(e => e.Name.LocalName == "Err");
                var code = firstErr.Descendants().FirstOrDefault(e => e.Name.LocalName == "Code")?.Value ?? "";
                var msg = firstErr.Descendants().FirstOrDefault(e => e.Name.LocalName == "Msg")?.Value ?? "";
                throw new Exception($"WSFE returned Errors. Code={code} Msg={msg}");
            }

            var cbteNroText = result.Descendants().FirstOrDefault(e => e.Name.LocalName == "CbteNro")?.Value?.Trim();
            if (!long.TryParse(cbteNroText, out var last))
                throw new Exception($"WSFE response does not contain a valid CbteNro. Value='{cbteNroText}'");

            return last;
        }
    }


}
