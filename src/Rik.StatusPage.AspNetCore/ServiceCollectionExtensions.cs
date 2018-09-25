using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Providers;

namespace Rik.StatusPage.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStatusPage(this IServiceCollection services, IConfiguration configuration, string sectionName = "StatusPage")
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(sectionName))
                throw new ArgumentNullException(nameof(sectionName));

            var statusPageSection = configuration.GetSection(sectionName);

            var statusPageOptions = new StatusPageOptions();
            statusPageSection.Bind(statusPageOptions);
            services.AddSingleton(statusPageOptions);

            foreach (var dependencySection in statusPageSection.GetSection("ExternalDependencies").GetChildren())
            {
                var provider = dependencySection.GetValue<string>("Provider");

                var statusProviderType = Type.GetType($"Rik.StatusPage.Providers.{provider}StatusProvider, Rik.StatusPage.AspNetCore");
                if (statusProviderType == null || statusProviderType.IsAbstract)
                    throw new Exception($"Invalid status provider name: {provider}.");

                var name = dependencySection.GetValue<string>("Name");

                var options = CreateStatusProviderOptions(statusProviderType, name);
                dependencySection.GetSection("Options").Bind(options);

                services.AddSingleton(typeof(IStatusProvider), Activator.CreateInstance(statusProviderType, options));
            }

            return services;
        }

        private static StatusProviderOptions CreateStatusProviderOptions(Type statusProviderType, string name)
        {
            var genericType = statusProviderType;
            while (genericType != null && (!genericType.IsGenericType || genericType.GetGenericTypeDefinition() != typeof(StatusProvider<>)))
                genericType = genericType.BaseType;

            if (genericType == null)
                throw new ArgumentException($"Status provider `{statusProviderType.FullName}` does not extend Rik.StatusPage.Providers.StatusProvider<> class.", nameof(statusProviderType));

            var optionsType = genericType.GetGenericArguments().Single();
            var options = (StatusProviderOptions)Activator.CreateInstance(optionsType, name);

            return options;
        }
    }
}