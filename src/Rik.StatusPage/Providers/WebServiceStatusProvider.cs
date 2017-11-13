using System;
using System.Net;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public class WebServiceStatusProvider : StatusProvider
    {
        private readonly string url;

        public WebServiceStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.Url))
                throw new ArgumentException("Web service url is required.", nameof(configuration.Url));

            url = configuration.Url;
        }

        protected override string GetUri()
        {
            return url;
        }

        protected override ExternalUnit OnCheckStatus(ExternalUnit externalUnit)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                return response.StatusCode == HttpStatusCode.OK
                    ? externalUnit.SetStatus(UnitStatus.Ok)
                    : externalUnit.SetStatus(UnitStatus.NotOk, "Status check failed (code: {0}; description: `{1}`)", response.StatusCode, response.StatusDescription);
            }
        }
    }
}