using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
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
    public class StatusModule : IHttpModule
    {
        public const string StatusPath = "~/status.xml";

        private static readonly string key = $"__Rik.StatusPage__{AppDomain.CurrentDomain.Id}__Exception__";
        private static readonly Func<string, StatusProviderConfigurationElement, IStatusProvider> statusProviderFactory;
        private static readonly Lazy<XmlSerializer> serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(Application)));
        private static readonly StatusPageConfigurationSection statusPageConfiguration = (StatusPageConfigurationSection)ConfigurationManager.GetSection("rik.statuspage");
        private static readonly XmlDocument document = new XmlDocument();

        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
            context.EndRequest += OnEndRequest;
        }

        public void Dispose()
        { }

        private static void OnBeginRequest(object sender, EventArgs args)
        {
            var application = (HttpApplication)sender;

            if (IsStatusPage(application.Context))
                application.Response.End();
        }

        private static void OnEndRequest(object sender, EventArgs args)
        {
            var application = (HttpApplication)sender;

            var isApplicationStartFailure = IsApplicationStartFailure(application);

            if (IsStatusPage(application.Context))
                WriteStatusPage(application.Context, isApplicationStartFailure ? UnitStatus.NotOk : UnitStatus.Ok);

            if (isApplicationStartFailure)
                HttpRuntime.UnloadAppDomain();
        }

        private static void WriteStatusPage(HttpContext context, UnitStatus applicationStatus)
        {
            context.Response.Clear();
            context.Response.ContentType = "text/xml";
            context.Response.ContentEncoding = Encoding.UTF8;

            var application = CollectStatus(context, applicationStatus);

            using (context.Response.Output)
                serializer.Value.Serialize(context.Response.Output, application, new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") }));
        }

        private static Application CollectStatus(HttpContext context, UnitStatus applicationStatus)
        {
            var externalStatusProviders =
                statusPageConfiguration?.StatusProviders
                    .OfType<StatusProviderConfigurationElement>()
                    .Select(x => statusProviderFactory(x.Provider, x))
                    .ToList()
                ?? new List<IStatusProvider>();

            var assemblyName = GetWebEntryAssembly(context).GetName();

            var externalUnits = new ConcurrentBag<ExternalUnit>();

            Parallel.ForEach(externalStatusProviders, async p =>
            {
                var status = await p.CheckStatusAsync();

                if (status != null)
                    externalUnits.Add(status);
            });

            var name = statusPageConfiguration?.Application.Name;
            var version = statusPageConfiguration?.Application.Version;

            return new Application
            {
                Name = string.IsNullOrWhiteSpace(name) ? assemblyName.Name : name,
                Version = string.IsNullOrWhiteSpace(version) ? assemblyName.Version.ToString() : version,
                Status = applicationStatus,
                ServerPlatform = GetServerPlatform(context),
                RuntimeEnvironment = GetRuntimeEnvironment(),
                ExternalDependencies = externalUnits.OrderBy(x => x.Name).ToArray(),
                AdditionalInfo = GetAdditionalInfo()
            };
        }

        public static bool IsStatusPage(HttpContext context)
        {
            var path = context.Request.AppRelativeCurrentExecutionFilePath ?? "";
            return StatusPath.Equals(path.ToLower());
        }

        public static bool IsApplicationStartFailure(HttpApplication application)
        {
            return application.Application.Get(key) is ObjectHandle objectHandle && objectHandle.Unwrap() != null;
        }

        public static void CaptureApplicationStartErrors(HttpApplication application, Action startAction)
        {
            try
            {
                startAction();
            }
            catch (Exception e)
            {
                application.Application[key] = new ObjectHandle(e);
            }
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

        private static Assembly GetWebEntryAssembly(HttpContext context)
        {
            if (context?.ApplicationInstance == null)
                return null;

            var type = context.ApplicationInstance.GetType();

            while (type != null && type.Namespace == "ASP")
                type = type.BaseType;

            return type == null ? null : type.Assembly;
        }

        private static XmlElement[] GetAdditionalInfo()
        {
            var additionalInfo = new List<XmlElement>();

            additionalInfo.AddRange(new[]
            {
                CreateElement("application_architecture", "value", Environment.Is64BitProcess ? "64-bit" : "32-bit"),
                CreateElement("os_version", "value", Environment.OSVersion.ToString()),
                CreateElement("os_architecture", "value", Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")
            });

            if (statusPageConfiguration.Application?.UnrecognizedElements.Any() == true)
                additionalInfo.AddRange(statusPageConfiguration.Application.UnrecognizedElements);

            return additionalInfo.ToArray();
        }

        private static XmlElement CreateElement(string name, string attributeName, string attributeValue)
        {
            var element = document.CreateElement(name);
            element.SetAttribute(attributeName, attributeValue);

            return element;
        }

        static StatusModule()
        {
            var mapping = new Dictionary<string, Type>();

            statusProviderFactory = (name, configuration) =>
            {
                if (mapping.TryGetValue(name, out var statusProviderType))
                    return (IStatusProvider) Activator.CreateInstance(statusProviderType, configuration);

                statusProviderType = Type.GetType($"Rik.StatusPage.Providers.{name}StatusProvider, Rik.StatusPage");
                if (statusProviderType == null || statusProviderType.IsAbstract)
                    throw new Exception($"Invalid status provider name: {name}.");

                mapping.Add(name, statusProviderType);

                return (IStatusProvider)Activator.CreateInstance(statusProviderType, configuration);
            };
        }
    }
}