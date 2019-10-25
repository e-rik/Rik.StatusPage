using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public class WebServiceStatusProvider : StatusProvider<WebServiceStatusProviderOptions>
    {
        public override string DisplayUri => options.Uri;

        public WebServiceStatusProvider(WebServiceStatusProviderOptions options)
            : base(options)
        { }

        protected override async Task<ExternalUnit> OnCheckStatusAsync(ExternalUnit externalUnit, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(options.Uri))
                return externalUnit.SetStatus(UnitStatus.NotOk, "Web service uri is required.");

            var request = (HttpWebRequest)WebRequest.Create(options.Uri);

            request.Method = "GET";
            request.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;

            using var response = (HttpWebResponse)await request.GetResponseAsync();

            return response.StatusCode == HttpStatusCode.OK
                ? externalUnit.SetStatus(UnitStatus.Ok)
                : externalUnit.SetStatus(UnitStatus.NotOk, "Status check failed (code: {0}; description: `{1}`)", response.StatusCode, response.StatusDescription);
        }
    }
}