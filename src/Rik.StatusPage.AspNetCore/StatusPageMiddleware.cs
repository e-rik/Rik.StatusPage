using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Providers;
using Rik.StatusPage.Schema;
using RuntimeEnvironment = Rik.StatusPage.Schema.RuntimeEnvironment;

namespace Rik.StatusPage.AspNetCore
{
    internal class StatusPageMiddleware
    {
        private static readonly XmlDocument document = new XmlDocument();
        private static readonly Lazy<XmlSerializer> serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(Application)));

        private readonly Func<IList<IStatusProvider>, CancellationToken, Task> statusProviderCustomization;
        private readonly Func<Application, CancellationToken, Task> resultCustomization;

        // ReSharper disable once UnusedParameter.Local
        public StatusPageMiddleware(RequestDelegate next, Func<IList<IStatusProvider>, CancellationToken, Task> statusProviderCustomization, Func<Application, CancellationToken, Task> resultCustomization)
        {
            this.resultCustomization = resultCustomization;
            this.statusProviderCustomization = statusProviderCustomization;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Response.ContentType = "text/xml";

            var application = await CollectStatusAsync(httpContext);

            using var stream = new MemoryStream();
            serializer.Value.Serialize(stream, application, new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") }));
            stream.Seek(0, SeekOrigin.Begin);

            await stream.CopyToAsync(httpContext.Response.Body);
        }

        private async Task<Application> CollectStatusAsync(HttpContext httpContext)
        {
            var statusPageOptions = httpContext.RequestServices.GetService<StatusPageOptions>();
            var statusProviders = httpContext.RequestServices.GetServices<IStatusProvider>().ToList();

            IStatusProvider mainStatusProvider = null;
            if (!string.IsNullOrEmpty(statusPageOptions?.Uri))
            {
                mainStatusProvider = new WebServiceStatusProvider(new WebServiceStatusProviderOptions("Main") { Uri = statusPageOptions.Uri });
                statusProviders.Insert(0, mainStatusProvider);
            }

            await statusProviderCustomization(statusProviders, default);

            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            var checkStatusTasks = statusProviders.Select(p => p.CheckStatusAsync(default));

            var statuses = await Task.WhenAll(checkStatusTasks);

            var mainStatus = mainStatusProvider != null ? statuses[0] : null;
            var externalDependencies = (mainStatusProvider != null ? statuses.Skip(1) : statuses).OrderBy(x => x.Name).ToArray();

            var name = statusPageOptions?.Name;
            var version = statusPageOptions?.Version;

            var application = new Application
            {
                Name = string.IsNullOrWhiteSpace(name) ? assemblyName.Name : name,
                Version = string.IsNullOrWhiteSpace(version) ? assemblyName.Version.ToString() : version,
                Status = mainStatus?.Status ?? UnitStatus.Ok,
                ServerPlatform = GetServerPlatform(),
                RuntimeEnvironment = GetRuntimeEnvironment(),
                ExternalDependencies = externalDependencies,
                AdditionalInfo = GetAdditionalInfo(statusPageOptions)
            };

            await resultCustomization(application, default);

            return application;
        }

        private static ServerPlatform GetServerPlatform()
        {
            var (platformName, platformVersion) = ParseNameAndVersion(RuntimeInformation.OSDescription?.Trim());

            return new ServerPlatform
            {
                Name = platformName,
                Version = platformVersion
            };
        }

        private static RuntimeEnvironment GetRuntimeEnvironment()
        {
            var (runtimeName, runtimeVersion) = ParseNameAndVersion(RuntimeInformation.FrameworkDescription?.Trim(), Environment.Version.ToString());

            return new RuntimeEnvironment
            {
                Name = runtimeName,
                Version = runtimeVersion
            };
        }

        private static XmlElement[] GetAdditionalInfo(StatusPageOptions statusPageOptions)
        {
            var additionalInfo = new List<XmlElement>();

            additionalInfo.AddRange(new[]
            {
                CreateElement("application_architecture", "value", Environment.Is64BitProcess ? "64-bit" : "32-bit"),
                CreateElement("os_version", "value", Environment.OSVersion.ToString()),
                CreateElement("os_architecture", "value", Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")
            });

            if (statusPageOptions?.AdditionalInfo?.Any() == true)
                additionalInfo.AddRange(statusPageOptions.AdditionalInfo.Select(x => CreateElement(x.Key, "value", x.Value)));

            return additionalInfo.ToArray();
        }

        private static XmlElement CreateElement(string name, string attributeName, string attributeValue)
        {
            var element = document.CreateElement(name);
            element.SetAttribute(attributeName, attributeValue);

            return element;
        }

        private static (string, string) ParseNameAndVersion(string input, string defaultVersion = null)
        {
            var name = "Unknown";
            var version = defaultVersion ?? "0";

            if (string.IsNullOrEmpty(input))
                return (name, version);

            var tokens = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var maybeVersion = tokens.Last();

            if (Regex.IsMatch(maybeVersion, @"^\d+(\.\d+)+"))
            {
                if (tokens.Length > 1)
                    name = string.Join(" ", tokens.Take(tokens.Length - 1));
                version = maybeVersion;
            }
            else name = input;

            return (name, version);
        }
    }
}