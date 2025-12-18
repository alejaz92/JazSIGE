using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.ARCA;
using FiscalDocumentationService.Business.Options;
using Microsoft.Extensions.Options;
using System.Security;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using static FiscalDocumentationService.Business.Exceptions.FiscalDocumentationException;

namespace FiscalDocumentationService.Business.Services.Clients
{
    public class ArcaAuthClient : IArcaAuthClient
    {
        private readonly HttpClient _http;
        private readonly ArcaOptions _options;
        private readonly IArcaAccessTicketCache _cache;

        public ArcaAuthClient(HttpClient http, IOptions<ArcaOptions> options, IArcaAccessTicketCache cache)
        {
            _http = http;
            _options = options.Value;
            _cache = cache;
        }
        public async Task<ArcaAccessTicket> GetAccessTicketAsync(string serviceName)
        {
            // 1) Cache
            var cached = _cache.Get(serviceName);
            if (cached != null && cached.IsValid())
                return cached;

            // 2) Validate config
            if (string.IsNullOrWhiteSpace(_options.Certificate.PfxPath))
                throw new InvalidOperationException("ARCA certificate path (Arca:Certificate:PfxPath) is not configured.");

            if (!File.Exists(_options.Certificate.PfxPath))
                throw new InvalidOperationException($"ARCA certificate file not found: {_options.Certificate.PfxPath}");

            var wsaaUrl = ResolveWsaaUrl();
            if (string.IsNullOrWhiteSpace(wsaaUrl))
                throw new InvalidOperationException("ARCA WSAA URL is not configured.");

            // 3) Build TRA XML
            var traXml = BuildTraXml(serviceName);

            // 4) Sign TRA (CMS/PKCS#7) and base64 encode
            var cmsBase64 = SignTraToCmsBase64(traXml, _options.Certificate.PfxPath, _options.Certificate.PfxPassword);

            // 5) Call WSAA LoginCms (SOAP)
            var soapEnvelope = BuildLoginCmsSoapEnvelope(cmsBase64);

            using var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            // SOAPAction header is not always required, but it doesn't hurt:
            content.Headers.Add("SOAPAction", "");

            var response = await _http.PostAsync(wsaaUrl, content);
            var responseXml = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ArcaDependencyException($"WSAA LoginCms error {(int)response.StatusCode}: {responseXml}");

            // 6) Parse SOAP -> loginCmsReturn (it contains XML of loginTicketResponse)
            var ltrXml = ExtractLoginCmsReturn(responseXml);

            var ticket = ParseLoginTicketResponse(ltrXml);


            // 7) Cache
            _cache.Set(serviceName, ticket);

            return ticket;
        }


        private string ResolveWsaaUrl()
        {
            var env = (_options.Environment ?? "Homologation").Trim();

            return env.Equals("Production", StringComparison.OrdinalIgnoreCase)
                ? _options.Wsaa.ProductionUrl
                : _options.Wsaa.HomologationUrl;
        }

        private static string BuildTraXml(string serviceName)
        {
            // Typical window: -5 minutes to +12 hours
            var now = DateTime.UtcNow;
            var gen = now.AddMinutes(-5);
            var exp = now.AddHours(12);

            // uniqueId: integer
            var uniqueId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var doc = new XDocument(
                new XElement("loginTicketRequest",
                    new XAttribute("version", "1.0"),
                    new XElement("header",
                        new XElement("uniqueId", uniqueId),
                        new XElement("generationTime", gen.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                        new XElement("expirationTime", exp.ToString("yyyy-MM-ddTHH:mm:ssZ"))
                    ),
                    new XElement("service", serviceName)
                )
            );

            return doc.Declaration + doc.ToString(SaveOptions.DisableFormatting);
        }

        private static string SignTraToCmsBase64(string traXml, string pfxPath, string pfxPassword)
        {
            var cert = new X509Certificate2(
                pfxPath,
                pfxPassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable
            );

            if (!cert.HasPrivateKey)
                throw new InvalidOperationException("The provided certificate does not have a private key.");

            var data = Encoding.UTF8.GetBytes(traXml);

            var contentInfo = new ContentInfo(data);
            var signedCms = new SignedCms(contentInfo, detached: true);

            var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, cert)
            {
                IncludeOption = X509IncludeOption.EndCertOnly
            };

            // You can add additional signed attributes if needed, but usually not required.
            signedCms.ComputeSignature(signer);

            var cms = signedCms.Encode();
            return Convert.ToBase64String(cms);
        }

        private static string BuildLoginCmsSoapEnvelope(string cmsBase64)
        {
            // SOAP 1.1 envelope
            return
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:wsaa=""http://wsaa.view.sua.dvadac.desein.afip.gov"">
  <soapenv:Header/>
  <soapenv:Body>
    <wsaa:loginCms>
      <wsaa:in0>{SecurityElement.Escape(cmsBase64)}</wsaa:in0>
    </wsaa:loginCms>
  </soapenv:Body>
</soapenv:Envelope>";
        }


        private static string ExtractLoginCmsReturn(string soapResponseXml)
        {
            var xml = XDocument.Parse(soapResponseXml);

            // Find element that ends with "loginCmsReturn"
            var node = xml.Descendants().FirstOrDefault(e => e.Name.LocalName == "loginCmsReturn");
            if (node == null)
                throw new Exception("WSAA response does not contain loginCmsReturn.");

            var ltr = node.Value?.Trim();
            if (string.IsNullOrWhiteSpace(ltr))
                throw new Exception("loginCmsReturn is empty.");

            return ltr;
        }

        private static ArcaAccessTicket ParseLoginTicketResponse(string loginTicketResponseXml)
        {
            var x = XDocument.Parse(loginTicketResponseXml);

            // token/sign are under credentials
            var token = x.Descendants().FirstOrDefault(e => e.Name.LocalName == "token")?.Value?.Trim();
            var sign = x.Descendants().FirstOrDefault(e => e.Name.LocalName == "sign")?.Value?.Trim();
            var expStr = x.Descendants().FirstOrDefault(e => e.Name.LocalName == "expirationTime")?.Value?.Trim();

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(sign))
                throw new Exception("loginTicketResponse does not contain token/sign.");

            if (string.IsNullOrWhiteSpace(expStr))
                throw new Exception("loginTicketResponse does not contain expirationTime.");

            // expirationTime comes as ISO8601 (usually with timezone)
            var exp = DateTime.Parse(expStr, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

            return new ArcaAccessTicket
            {
                Token = token,
                Sign = sign,
                ExpirationTimeUtc = exp.ToUniversalTime()
            };
        }
    }
}
