using FiscalDocumentationService.Business.Interfaces.Clients;
using FiscalDocumentationService.Business.Models.ARCA;
using FiscalDocumentationService.Business.Options;
using Microsoft.Extensions.Options;
using System.Security;
using System.Security.Cryptography;
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
            // 1) Cache (fast path)
            var cached = _cache.Get(serviceName);
            if (cached != null && cached.IsValid())
                return cached;

            // 2) Lock per serviceName (avoid concurrent TA requests)
            var sem = _cache.GetLock(serviceName);
            await sem.WaitAsync();
            try
            {
                // 3) Double-check cache inside lock
                cached = _cache.Get(serviceName);
                if (cached != null && cached.IsValid())
                    return cached;

                // 4) Validate config
                if (string.IsNullOrWhiteSpace(_options.Certificate.PfxPath))
                    throw new InvalidOperationException("ARCA certificate path (Arca:Certificate:PfxPath) is not configured.");

                if (!File.Exists(_options.Certificate.PfxPath))
                    throw new InvalidOperationException($"ARCA certificate file not found: {_options.Certificate.PfxPath}");

                var wsaaUrl = ResolveWsaaUrl();
                if (string.IsNullOrWhiteSpace(wsaaUrl))
                    throw new InvalidOperationException("ARCA WSAA URL is not configured.");

                // 5) Build + sign
                var traXml = BuildTraXml(serviceName);
                var cmsBase64 = SignTraToCmsBase64(traXml, _options.Certificate.PfxPath, _options.Certificate.PfxPassword);
                var soapEnvelope = BuildLoginCmsSoapEnvelope(cmsBase64);

                // 6) Call WSAA
                var (ok, xml, status) = await PostLoginCmsAsync(wsaaUrl, soapEnvelope);

                if (!ok)
                {
                    // alreadyAuthenticated: no tiene sentido reintentar con el MISMO request en paralelo.
                    // Si pasa, es porque el TA existe en WSAA pero nosotros no lo tenemos (reinicio/cache vacío).
                    if (xml.Contains("coe.alreadyAuthenticated", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArcaDependencyException(
                            "WSAA indica que ya existe un TA válido (coe.alreadyAuthenticated), " +
                            "pero este servicio no lo tiene en cache (posible reinicio). " +
                            "Esperá a que venza el TA o reiniciá el proceso cuando corresponda.");
                    }

                    throw new ArcaDependencyException($"WSAA LoginCms error {(int)status}: {xml}");
                }

                // 7) Parse + cache
                var ltrXml = ExtractLoginCmsReturn(xml);
                var ticket = ParseLoginTicketResponse(ltrXml);

                _cache.Set(serviceName, ticket);
                return ticket;
            }
            finally
            {
            known: sem.Release();
            }
        }

        private async Task<(bool ok, string xml, System.Net.HttpStatusCode status)> PostLoginCmsAsync(string wsaaUrl, string soapEnvelope)
        {
            using var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "");

            var response = await _http.PostAsync(wsaaUrl, content);
            var responseXml = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, responseXml, response.StatusCode);
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
                            X509KeyStorageFlags.UserKeySet |
                            X509KeyStorageFlags.PersistKeySet |
                            X509KeyStorageFlags.Exportable
                        );

            var rsa = cert.GetRSAPrivateKey();
            if (rsa == null)
                throw new InvalidOperationException("Private key not accessible (GetRSAPrivateKey returned null).");


            var keySize = rsa.KeySize;
            if (keySize < 2048) throw new InvalidOperationException($"RSA key too small: {keySize} bits.");

            if (!cert.HasPrivateKey)
                throw new InvalidOperationException("The provided certificate does not have a private key.");

            var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var data = utf8NoBom.GetBytes(traXml);


            var contentInfo = new ContentInfo(data);
            var signedCms = new SignedCms(contentInfo, detached: false);

            var signer = new CmsSigner(
                SubjectIdentifierType.SubjectKeyIdentifier,
                cert)
            {
                IncludeOption = X509IncludeOption.EndCertOnly,
                DigestAlgorithm = new Oid("1.3.14.3.2.26") // SHA-1
            };






            // You can add additional signed attributes if needed, but usually not required.
            signedCms.ComputeSignature(signer);

            var cms = signedCms.Encode();
            var cmsBase64 = Convert.ToBase64String(cms); // sin InsertLineBreaks
            return cmsBase64;


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
