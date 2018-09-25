using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public class XRoadProducerStatusProvider : StatusProvider<XRoadProducerStatusProviderOptions>
    {
        private const string SOAP_ENC_NAMESPACE = "http://schemas.xmlsoap.org/soap/encoding/";
        private const string SOAP_ENV_NAMESPACE = "http://schemas.xmlsoap.org/soap/envelope/";

        private const string XRD_20_NAMESPACE = "http://x-tee.riik.ee/xsd/xtee.xsd";
        private const string XRD_30_NAMESPACE = "http://x-rd.net/xsd/xroad.xsd";
        private const string XRD_31_NAMESPACE = "http://x-road.ee/xsd/x-road.xsd";

        private bool IsLegacy => "2.0".Equals(options.ProtocolVersion);

        public override string DisplayUri => $"{options.ProducerName}@{options.Uri}";

        public XRoadProducerStatusProvider(XRoadProducerStatusProviderOptions options)
            : base(options)
        {
            if (IsValidProtocolVersion(options.ProtocolVersion))
                throw new ArgumentException("X-Road protocol version is required.", nameof(options.ProtocolVersion));

            if (string.IsNullOrWhiteSpace(options.Uri))
                throw new ArgumentException("X-Road security server uri is required.", nameof(options.Uri));

            if (string.IsNullOrWhiteSpace(options.ProducerName))
                throw new ArgumentException("X-Road producer name is required.", nameof(options.ProducerName));

            if (string.IsNullOrWhiteSpace(options.Consumer))
                throw new ArgumentException("X-Road consumer is required.", nameof(options.Consumer));

            if (string.IsNullOrWhiteSpace(options.UserId))
                throw new ArgumentException("X-Road user id is required.", nameof(options.UserId));
        }

        protected override async Task<ExternalUnit> OnCheckStatusAsync(ExternalUnit externalUnit)
        {
            externalUnit.ServerPlatform = new ServerPlatform { Name = "X-Road" };

            var statusCode = await GetStatusCodeAsync();

            return statusCode == 1
                ? externalUnit.SetStatus(UnitStatus.Ok)
                : externalUnit.SetStatus(UnitStatus.NotOk, $"Status check returned {statusCode} as result.");
        }

        private static bool IsValidProtocolVersion(string protocolVersion)
        {
            IList<string> acceptedProtocolVersions = new [] { "2.0", "3.0", "3.1" };

            return acceptedProtocolVersions.Contains(protocolVersion);
        }

        private async Task<int> GetStatusCodeAsync()
        {
            const string soapenv = "soapenv";
            const string xrd = "xrd";

            var request = WebRequest.Create(options.Uri);
            request.Method = "POST";
            request.ContentType = $"text/xml; charset={Encoding.UTF8.HeaderName}";
            request.Headers.Set("SOAPAction", "");

            var xrdNamespace = GetXRoadNamespace();

            using (var stream = await request.GetRequestStreamAsync())
            using (var writer = XmlWriter.Create(stream))
            {
                await writer.WriteStartDocumentAsync();
                await writer.WriteStartElementAsync(soapenv, "Envelope", SOAP_ENV_NAMESPACE);

                if (IsLegacy)
                    await writer.WriteAttributeStringAsync(soapenv, "encodingStyle", SOAP_ENV_NAMESPACE, SOAP_ENC_NAMESPACE);

                await writer.WriteStartElementAsync(soapenv, "Header", SOAP_ENV_NAMESPACE);
                await writer.WriteElementStringAsync(xrd, IsLegacy ? "asutus" : "consumer", xrdNamespace, options.Consumer ?? "");
                await writer.WriteElementStringAsync(xrd, IsLegacy ? "andmekogu" : "producer", xrdNamespace, options.ProducerName);
                await writer.WriteElementStringAsync(xrd, IsLegacy ? "isikukood" : "userId", xrdNamespace, options.UserId);
                await writer.WriteElementStringAsync(xrd, "id", xrdNamespace, GenerateNonce());
                await writer.WriteElementStringAsync(xrd, IsLegacy ? "nimi" : "service", xrdNamespace, $"{options.ProducerName}.getState");
                await writer.WriteElementStringAsync(xrd, IsLegacy ? "toimik" : "issue", xrdNamespace, "");
                await writer.WriteEndElementAsync();

                await writer.WriteStartElementAsync(soapenv, "Body", SOAP_ENV_NAMESPACE);
                await writer.WriteElementStringAsync(xrd, "getState", xrdNamespace, "");
                await writer.WriteEndElementAsync();

                await writer.WriteEndElementAsync();
                await writer.WriteEndDocumentAsync();
            }

            using (var response = await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null)
                    throw new Exception("Could not get response from security server.");

                var document = new XPathDocument(responseStream);
                var navigator = document.CreateNavigator();

                var manager = new XmlNamespaceManager(navigator.NameTable ?? new NameTable());
                manager.AddNamespace(soapenv, SOAP_ENV_NAMESPACE);
                manager.AddNamespace(xrd, xrdNamespace);

                var selector = $"//soapenv:Envelope/soapenv:Body/xrd:getStateResponse/{("2.0".Equals(options.ProtocolVersion) ? "keha" : "response")}";

                var resultNode = navigator.SelectSingleNode(selector, manager);
                if (resultNode == null || !Regex.IsMatch(resultNode.Value, @"\d+"))
                    throw new Exception("Invalid response.");

                return resultNode.ValueAsInt;
            }
        }

        private string GetXRoadNamespace()
        {
            switch (options.ProtocolVersion)
            {
                case "2.0":
                    return XRD_20_NAMESPACE;

                case "3.0":
                    return XRD_30_NAMESPACE;

                case "3.1":
                    return XRD_31_NAMESPACE;

                default:
                    throw new InvalidProgramException("never");
            }
        }

        private static string GenerateNonce()
        {
            var nonce = new byte[42];
            var rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(nonce);

            return Convert.ToBase64String(nonce);
        }
    }
}