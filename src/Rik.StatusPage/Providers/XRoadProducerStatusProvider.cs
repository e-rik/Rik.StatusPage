using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public enum XRoadProtocol { Version20, Version30, Version31, Version40 }

    public class XRoadProducerStatusProvider : StatusProvider
    {
        private const string SOAP_ENC_NAMESPACE = "http://schemas.xmlsoap.org/soap/encoding/";
        private const string SOAP_ENV_NAMESPACE = "http://schemas.xmlsoap.org/soap/envelope/";

        private const string XRD_20_NAMESPACE = "http://x-tee.riik.ee/xsd/xtee.xsd";
        private const string XRD_30_NAMESPACE = "http://x-rd.net/xsd/xroad.xsd";
        private const string XRD_31_NAMESPACE = "http://x-road.ee/xsd/x-road.xsd";

        private readonly XRoadProtocol protocol;
        private readonly string securityServer;
        private readonly string producerName;
        private readonly string consumer;
        private readonly string userId;

        private bool IsLegacy => protocol == XRoadProtocol.Version20;

        public XRoadProducerStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.Protocol))
                throw new ArgumentException("X-Road protocol version is required.", nameof(configuration.Protocol));

            if (string.IsNullOrWhiteSpace(configuration.SecurityServer))
                throw new ArgumentException("X-Road security server uri is required.", nameof(configuration.SecurityServer));

            if (string.IsNullOrWhiteSpace(configuration.ProducerName))
                throw new ArgumentException("X-Road producer name is required.", nameof(configuration.ProducerName));

            if (string.IsNullOrWhiteSpace(configuration.Consumer))
                throw new ArgumentException("X-Road consumer is required.", nameof(configuration.Consumer));

            if (string.IsNullOrWhiteSpace(configuration.UserId))
                throw new ArgumentException("X-Road user id is required.", nameof(configuration.UserId));

            protocol = GetProtocolVersion(configuration.Protocol);
            securityServer = configuration.SecurityServer;
            producerName = configuration.ProducerName;
            consumer = configuration.Consumer;
            userId = configuration.UserId;
        }

        protected override string GetUri()
        {
            return $"{producerName}@{securityServer}";
        }

        protected override ExternalUnit OnCheckStatus(ExternalUnit externalUnit)
        {
            externalUnit.ServerPlatform = new ServerPlatform { Name = "X-Road" };

            var statusCode = GetStatusCode();

            return statusCode == 1
                ? externalUnit.SetStatus(UnitStatus.Ok)
                : externalUnit.SetStatus(UnitStatus.NotOk, $"Status check returned {statusCode} as result.");
        }

        private static XRoadProtocol GetProtocolVersion(string protocolVersion)
        {
            switch (protocolVersion)
            {
                case "2.0":
                    return XRoadProtocol.Version20;
                case "3.0":
                    return XRoadProtocol.Version30;
                case "3.1":
                    return XRoadProtocol.Version31;
                case "4.0":
                    return XRoadProtocol.Version40;
                default:
                    throw new Exception($"Unknown XRoad protocol version `{protocolVersion}`.");
            }
        }

        private int GetStatusCode()
        {
            var request = WebRequest.Create(securityServer);
            request.Method = "POST";
            request.ContentType = $"text/xml; charset={Encoding.UTF8.HeaderName}";
            request.Headers.Set("SOAPAction", "");

            var xrdNamespace = GetXRoadNamespace();

            using (var stream = request.GetRequestStream())
            using (var writer = XmlWriter.Create(stream))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("soapenv", "Envelope", SOAP_ENV_NAMESPACE);

                if (IsLegacy)
                    writer.WriteAttributeString("encodingStyle", SOAP_ENV_NAMESPACE, SOAP_ENC_NAMESPACE);

                writer.WriteStartElement("Header", SOAP_ENV_NAMESPACE);
                writer.WriteElementString("xrd", IsLegacy ? "asutus" : "consumer", xrdNamespace, consumer ?? "");
                writer.WriteElementString("xrd", IsLegacy ? "andmekogu" : "producer", xrdNamespace, producerName);
                writer.WriteElementString("xrd", IsLegacy ? "isikukood" : "userId", xrdNamespace, userId);
                writer.WriteElementString("xrd", "id", xrdNamespace, GenerateNonce());
                writer.WriteElementString("xrd", IsLegacy ? "nimi" : "service", xrdNamespace, $"{producerName}.getState");
                writer.WriteElementString("xrd", IsLegacy ? "toimik" : "issue", xrdNamespace, "");
                writer.WriteEndElement();

                writer.WriteStartElement("Body", SOAP_ENV_NAMESPACE);
                writer.WriteElementString("xrd", "getState", xrdNamespace, "");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null)
                    throw new Exception("Could not get response from security server.");

                var document = new XPathDocument(responseStream);
                var navigator = document.CreateNavigator();

                var manager = new XmlNamespaceManager(navigator.NameTable ?? new NameTable());
                manager.AddNamespace("soapenv", SOAP_ENV_NAMESPACE);
                manager.AddNamespace("xrd", xrdNamespace);

                var selector = $"//soapenv:Envelope/soapenv:Body/xrd:getStateResponse/{(protocol == XRoadProtocol.Version20 ? "keha" : "response")}";

                var resultNode = navigator.SelectSingleNode(selector, manager);
                if (resultNode == null || !Regex.IsMatch(resultNode.Value, @"\d+"))
                    throw new Exception("Invalid response.");

                return resultNode.ValueAsInt;
            }
        }

        private string GetXRoadNamespace()
        {
            switch (protocol)
            {
                case XRoadProtocol.Version20:
                    return XRD_20_NAMESPACE;
                case XRoadProtocol.Version30:
                    return XRD_30_NAMESPACE;
                case XRoadProtocol.Version31:
                    return XRD_31_NAMESPACE;
                case XRoadProtocol.Version40:
                    throw new NotImplementedException();
                default:
                    throw new Exception("Unsupported X-Road protocol version.");
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