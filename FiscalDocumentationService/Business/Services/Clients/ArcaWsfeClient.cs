using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.Arca;
using FiscalDocumentationService.Business.Options;
using Microsoft.Extensions.Options;
using System.Globalization;
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
        
        public async Task<CaeResponse> RequestCaeAsync(long issuerCuit, WsfeCaeRequest req, CancellationToken ct = default)
        {
            EnsureEmissionAllowed();

            var url = ResolveWsfeUrl();
            var auth = await BuildAuthAsync(issuerCuit, ct);

            var dateStr = req.CbteDate.ToString("yyyyMMdd");

            ValidateCaeRequest(req);

            var vatXml = BuildVatXml(req);
            var referenceXml = BuildReferenceXml(req);

            var soapEnvelope =
        $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ar=""http://ar.gov.afip.dif.FEV1/"">
  <soapenv:Header/>
  <soapenv:Body>
    <ar:FECAESolicitar>
      <ar:Auth>
        <ar:Token>{SecurityElementEscape(auth.Token)}</ar:Token>
        <ar:Sign>{SecurityElementEscape(auth.Sign)}</ar:Sign>
        <ar:Cuit>{auth.Cuit}</ar:Cuit>
      </ar:Auth>
      <ar:FeCAEReq>
        <ar:FeCabReq>
          <ar:CantReg>1</ar:CantReg>
          <ar:PtoVta>{req.PointOfSale}</ar:PtoVta>
          <ar:CbteTipo>{req.CbteType}</ar:CbteTipo>
        </ar:FeCabReq>
        <ar:FeDetReq>
          <ar:FECAEDetRequest>
            <ar:Concepto>{req.Concept}</ar:Concepto>
            <ar:DocTipo>{req.DocType}</ar:DocTipo>
            <ar:DocNro>{req.DocNumber}</ar:DocNro>
            <ar:CbteDesde>{req.CbteFrom}</ar:CbteDesde>
            <ar:CbteHasta>{req.CbteTo}</ar:CbteTo>
            <ar:CbteFch>{dateStr}</ar:CbteFch>
            <ar:ImpTotal>{FormatAmount(req.TotalAmount)}</ar:ImpTotal>
            <ar:ImpTotConc>{FormatAmount(req.NotTaxedAmount)}</ar:ImpTotConc>
            <ar:ImpNeto>{FormatAmount(req.NetAmount)}</ar:ImpNeto>
            <ar:ImpOpEx>{FormatAmount(req.ExemptAmount)}</ar:ImpOpEx>
            <ar:ImpIVA>{FormatAmount(req.VatAmount)}</ar:ImpIVA>
            <ar:ImpTrib>{FormatAmount(req.OtherTaxesAmount)}</ar:ImpTrib>
            <ar:MonId>{SecurityElementEscape(req.CurrencyId)}</ar:MonId>
            <ar:MonCotiz>{FormatAmount(req.CurrencyRate)}</ar:MonCotiz>
            <ar:CondicionIVAReceptorId>{req.ReceiverVatConditionId}</ar:CondicionIVAReceptorId>

            {vatXml}
            {referenceXml}
          </ar:FECAEDetRequest>
        </ar:FeDetReq>
      </ar:FeCAEReq>
    </ar:FECAESolicitar>
  </soapenv:Body>
</soapenv:Envelope>";

            using var httpReq = new HttpRequestMessage(HttpMethod.Post, url);
            httpReq.Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            httpReq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            httpReq.Headers.Add("SOAPAction", "\"http://ar.gov.afip.dif.FEV1/FECAESolicitar\"");

            using var resp = await _http.SendAsync(httpReq, ct);
            var xml = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"WSFE FECAESolicitar error {(int)resp.StatusCode}: {xml}");

            return ParseCaeResponse(xml);
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

        private void EnsureEmissionAllowed()
        {
            var options = _arcaOptions.Value;
            if (string.Equals(options.Environment, "Production", StringComparison.OrdinalIgnoreCase)
                && !options.AllowProductionEmission)
            {
                throw new InvalidOperationException("Production emission is blocked by configuración (AllowProductionEmission=false).");
            }
        }

        private static string BuildVatXml(WsfeCaeRequest req)
        {
            if (req.VatItems == null || req.VatItems.Count == 0)
                return ""; // For B/C usually ok

            var items = string.Join("", req.VatItems.Select(v =>
        $@"<ar:AlicIva>
  <ar:Id>{v.VatId}</ar:Id>
  <ar:BaseImp>{FormatAmount(v.BaseAmount)}</ar:BaseImp>
  <ar:Importe>{FormatAmount(v.VatAmount)}</ar:Importe>
</ar:AlicIva>"));

            return $@"<ar:Iva>{items}</ar:Iva>";
        }

        private static string BuildReferenceXml(WsfeCaeRequest req)
        {
            // Campos de referencia solo para notas de débito (2, 7, 12, 52) y crédito (3, 8, 13, 53)
            // Tipos de comprobante: 1,6,11,51 = Facturas; 2,7,12,52 = Notas de Débito; 3,8,13,53 = Notas de Crédito
            var isNote = req.CbteType == 2 || req.CbteType == 7 || req.CbteType == 12 || req.CbteType == 52 ||
                         req.CbteType == 3 || req.CbteType == 8 || req.CbteType == 13 || req.CbteType == 53;

            if (!isNote || !req.ReferencedCbteType.HasValue || !req.ReferencedPointOfSale.HasValue || !req.ReferencedCbteNumber.HasValue)
                return "";

            return $@"
            <ar:CbtesAsoc>
              <ar:CbteAsoc>
                <ar:Tipo>{req.ReferencedCbteType.Value}</ar:Tipo>
                <ar:PtoVta>{req.ReferencedPointOfSale.Value}</ar:PtoVta>
                <ar:Nro>{req.ReferencedCbteNumber.Value}</ar:Nro>
              </ar:CbteAsoc>
            </ar:CbtesAsoc>";
        }

        private static string FormatAmount(decimal value)
            => value.ToString("0.00", CultureInfo.InvariantCulture);


        private static CaeResponse ParseCaeResponse(string soapXml)
        {
            var x = XDocument.Parse(soapXml);

            var resultNode = x.Descendants().FirstOrDefault(e => e.Name.LocalName == "FECAESolicitarResult");
            if (resultNode == null)
                throw new Exception("WSFE response does not contain FECAESolicitarResult.");

            var det = resultNode.Descendants().FirstOrDefault(e => e.Name.LocalName == "FECAEDetResponse");

            string result = det?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Resultado")?.Value?.Trim() ?? "";
            string? cae = det?.Descendants().FirstOrDefault(e => e.Name.LocalName == "CAE")?.Value?.Trim();
            string? caeDue = det?.Descendants().FirstOrDefault(e => e.Name.LocalName == "CAEFchVto")?.Value?.Trim();

            var errors = new List<WsfeError>();
            var events = new List<WsfeEvent>();

            // 1) Errors en nivel raíz (a veces están)
            AppendErrors(resultNode, errors);

            // 2) Errors en nivel detalle (muy común)
            if (det != null) AppendErrors(det, errors);

            // 3) Observaciones (MUY común para rechazos)
            if (det != null) AppendObservations(det, errors);

            // Events (pueden estar en raíz o detalle)
            AppendEvents(resultNode, events);
            if (det != null) AppendEvents(det, events);

            return new CaeResponse(result, cae, caeDue, errors, events);
        }

        private static void AppendErrors(XElement parent, List<WsfeError> errors)
        {
            var errorsNode = parent.Descendants().FirstOrDefault(e => e.Name.LocalName == "Errors");
            if (errorsNode == null) return;

            foreach (var err in errorsNode.Descendants().Where(e => e.Name.LocalName == "Err"))
            {
                var code = err.Descendants().FirstOrDefault(e => e.Name.LocalName == "Code")?.Value ?? "";
                var msg = err.Descendants().FirstOrDefault(e => e.Name.LocalName == "Msg")?.Value ?? "";
                if (!string.IsNullOrWhiteSpace(code) || !string.IsNullOrWhiteSpace(msg))
                    errors.Add(new WsfeError(code, msg));
            }
        }

        private static void AppendObservations(XElement det, List<WsfeError> errors)
        {
            var obsNode = det.Descendants().FirstOrDefault(e => e.Name.LocalName == "Observaciones");
            if (obsNode == null) return;

            foreach (var obs in obsNode.Descendants().Where(e => e.Name.LocalName == "Obs"))
            {
                var code = obs.Descendants().FirstOrDefault(e => e.Name.LocalName == "Code")?.Value ?? "";
                var msg = obs.Descendants().FirstOrDefault(e => e.Name.LocalName == "Msg")?.Value ?? "";
                if (!string.IsNullOrWhiteSpace(code) || !string.IsNullOrWhiteSpace(msg))
                    errors.Add(new WsfeError(code, msg));
            }
        }

        private static void AppendEvents(XElement parent, List<WsfeEvent> events)
        {
            var eventsNode = parent.Descendants().FirstOrDefault(e => e.Name.LocalName == "Events");
            if (eventsNode == null) return;

            foreach (var ev in eventsNode.Descendants().Where(e => e.Name.LocalName == "Evt"))
            {
                var code = ev.Descendants().FirstOrDefault(e => e.Name.LocalName == "Code")?.Value ?? "";
                var msg = ev.Descendants().FirstOrDefault(e => e.Name.LocalName == "Msg")?.Value ?? "";
                if (!string.IsNullOrWhiteSpace(code) || !string.IsNullOrWhiteSpace(msg))
                    events.Add(new WsfeEvent(code, msg));
            }
        }

        private static void ValidateCaeRequest(WsfeCaeRequest req)
        {
            // Regla de totales: ImpTotal = ImpTotConc + ImpNeto + ImpOpEx + ImpTrib + ImpIVA
            var expectedTotal =
                req.NotTaxedAmount +
                req.NetAmount +
                req.ExemptAmount +
                req.OtherTaxesAmount +
                req.VatAmount;

            if (decimal.Round(req.TotalAmount, 2) != decimal.Round(expectedTotal, 2))
                throw new Exception($"Invalid totals. Total={req.TotalAmount:0.00} Expected={expectedTotal:0.00}");

            // Si hay neto gravado, IVA debe estar informado
            if (req.NetAmount > 0)
            {
                if (req.VatItems == null || req.VatItems.Count == 0)
                    throw new Exception("VatItems is required when NetAmount > 0.");

                var sumVat = req.VatItems.Sum(x => x.VatAmount);
                if (decimal.Round(sumVat, 2) != decimal.Round(req.VatAmount, 2))
                    throw new Exception($"VatAmount mismatch. VatAmount={req.VatAmount:0.00} Sum(VatItems)={sumVat:0.00}");
            }
        }


    }


}
