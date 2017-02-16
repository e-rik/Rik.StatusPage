using System;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage
{
    public class StatusHandler : IHttpHandler
    {
        private static readonly Lazy<XmlSerializer> serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(Application)));
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Clear();
            context.Response.ContentType = "text/xml";

            var application = CollectStatus(context);

            using (context.Response.Output)
                serializer.Value.Serialize(context.Response.Output, application, new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") }));
        }

        protected virtual Application CollectStatus(HttpContext context)
        {
            return new Application();
        }
    }
}