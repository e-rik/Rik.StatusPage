using System;
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
            var asyncHelper = new EventHandlerTaskAsyncHelper(WriteStatusResponseAsync);

            context.BeginRequest += OnBeginRequest;

            context.AddOnEndRequestAsync(asyncHelper.BeginEventHandler, asyncHelper.EndEventHandler);
        }

        public void Dispose()
        { }

        private static void OnBeginRequest(object sender, EventArgs args)
        {
            var application = (HttpApplication)sender;

            if (IsStatusPage(application.Context))
                application.Response.End();
        }

        private static async Task WriteStatusResponseAsync(object sender, EventArgs args)
        {
            var application = (HttpApplication)sender;

            var isApplicationStartFailure = IsApplicationStartFailure(application);

            if (IsStatusPage(application.Context))
                await WriteStatusPageAsync(application.Context, isApplicationStartFailure ? UnitStatus.NotOk : UnitStatus.Ok);

            if (isApplicationStartFailure)
                HttpRuntime.UnloadAppDomain();
        }

        private static async Task WriteStatusPageAsync(HttpContext context, UnitStatus applicationStatus)
        {
            context.Response.Clear();
            context.Response.ContentType = "text/xml";
            context.Response.ContentEncoding = Encoding.UTF8;

            var application = await CollectStatusAsync(context, applicationStatus);

            using (context.Response.Output)
                serializer.Value.Serialize(context.Response.Output, application, new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") }));
        }

        private static async Task<Application> CollectStatusAsync(HttpContext context, UnitStatus applicationStatus)
        {
            var externalStatusProviders =
                statusPageConfiguration?.StatusProviders
                    .OfType<StatusProviderConfigurationElement>()
                    .Select(x => statusProviderFactory(x.Provider, x))
                    .ToList()
                ?? new List<IStatusProvider>();

            var assemblyName = GetWebEntryAssembly(context).GetName();

            var checkStatusTasks = externalStatusProviders.Select(p => p.CheckStatusAsync(default));
            var externalUnits = await Task.WhenAll(checkStatusTasks);

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
                var isCustomProvider = TryGetCustomProviderTypeName(name, configuration, out var customTypeName);
                var mappingKey = isCustomProvider ? $"Custom#{customTypeName}" : name;

                if (mapping.TryGetValue(mappingKey, out var statusProviderType))
                    return (IStatusProvider)Activator.CreateInstance(statusProviderType, InitializeStatusProviderOptions(statusProviderType, configuration));

                statusProviderType = isCustomProvider
                    ? Type.GetType(customTypeName)
                    : Type.GetType($"Rik.StatusPage.Providers.{name}StatusProvider, Rik.StatusPage");

                if (statusProviderType == null || statusProviderType.IsAbstract)
                    throw new Exception($"Invalid status provider configuration: {mappingKey}.");

                mapping.Add(mappingKey, statusProviderType);

                return (IStatusProvider)Activator.CreateInstance(statusProviderType, InitializeStatusProviderOptions(statusProviderType, configuration));
            };
        }

        private static bool TryGetCustomProviderTypeName(string name, StatusProviderConfigurationElement configurationElement, out string typeName)
        {
            typeName = null;

            if (!"Custom".Equals(name))
                return false;

            var classKey = configurationElement.UnrecognizedAttributes.Keys.SingleOrDefault(x => "class".Equals(x.ToLower()));
            var classValue = classKey != null ? configurationElement.UnrecognizedAttributes[classKey] : null;

            if (string.IsNullOrEmpty(classValue))
                throw new ArgumentNullException(nameof(name), @"Custom provider element must define class attribute which refers to provider type.");

            typeName = classValue;

            return true;
        }

        private static StatusProviderOptions InitializeStatusProviderOptions(Type statusProviderType, StatusProviderConfigurationElement configurationElement)
        {
            var genericType = statusProviderType;
            while (genericType != null && (!genericType.IsGenericType || genericType.GetGenericTypeDefinition() != typeof(StatusProvider<>)))
                genericType = genericType.BaseType;

            if (genericType == null)
                throw new ArgumentException($"Status provider `{statusProviderType.FullName}` does not extend Rik.StatusPage.Providers.StatusProvider<> class.", nameof(statusProviderType));

            var optionsType = genericType.GetGenericArguments().Single();
            var options = (StatusProviderOptions)Activator.CreateInstance(optionsType, configurationElement.Name);

            var attributes = configurationElement.UnrecognizedAttributes.ToDictionary(x => x.Key.ToLower(), x => x.Value);
            foreach (var propertyInfo in optionsType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite))
                if (attributes.ContainsKey(propertyInfo.Name.ToLower()))
                    propertyInfo.SetValue(options, Convert.ChangeType(attributes[propertyInfo.Name.ToLower()].GetAppSettingsOrValue(), propertyInfo.PropertyType));

            return options;
        }
    }
}