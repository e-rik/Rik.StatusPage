﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Providers;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage
{
    public class StatusHandler : IHttpHandler
    {
        private static readonly Lazy<XmlSerializer> serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(Application)));
        private static readonly StatusPageConfigurationSection statusPageConfiguration = (StatusPageConfigurationSection)ConfigurationManager.GetSection("rik.statuspage");

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Clear();
            context.Response.ContentType = "text/xml";
            context.Response.ContentEncoding = Encoding.UTF8;

            var application = CollectStatus(context);

            using (context.Response.Output)
                serializer.Value.Serialize(context.Response.Output, application, new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") }));
        }

        protected virtual Application CollectStatus(HttpContext context)
        {
            var externalStatusProviders = new List<StatusProvider>();

            foreach (StatusProviderConfigurationElement statusProvider in statusPageConfiguration.StatusProviders)
            {
                switch (statusProvider.Type)
                {
                    case StatusProviderType.Database:
                        externalStatusProviders.Add(new DatabaseStatusProvider(statusProvider.Name, statusProvider.ConnectionString, statusProvider.ConnectionType));
                        break;
                }
            }

            var externalUnits = new ConcurrentBag<ExternalUnit>();

            Parallel.ForEach(externalStatusProviders, p =>
            {
                var status = p.CheckStatus();

                if (status != null)
                    externalUnits.Add(status);
            });

            return new Application
            {
                Name = statusPageConfiguration.Application.Name,
                Version = statusPageConfiguration.Application.Version,
                Status = UnitStatus.Ok,
                ServerPlatform = GetServerPlatform(context),
                RuntimeEnvironment = GetRuntimeEnvironment(),
                ExternalDependencies = externalUnits.OrderBy(x => x.Name).ToArray(),
            };
        }

        private static ServerPlatform GetServerPlatform(HttpContext context)
        {
            var serverSoftware = context.Request.ServerVariables["SERVER_SOFTWARE"].Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            return new ServerPlatform
            {
                Name = serverSoftware.Length > 0 ? serverSoftware[0] : string.Empty,
                Version = serverSoftware.Length > 1 ? serverSoftware[1] : string.Empty
            };
        }

        private static RuntimeEnvironment GetRuntimeEnvironment()
        {
            var runtimeName = Type.GetType("Mono.Runtime") != null ? "Mono" : ".NET";

            return new RuntimeEnvironment
            {
                Name = runtimeName,
                Version = Environment.Version.ToString()
            };
        }
    }
}