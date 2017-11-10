using System;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public class CustomStatusProvider : StatusProvider
    {
        private readonly StatusProvider customStatusProvider;

        public CustomStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.Class))
                throw new ArgumentException("Class name is required for custom provider.", nameof(configuration.Class));

            var statusProviderType = FindType(configuration.Class);

            if (statusProviderType == null)
                throw new Exception($"Cannot load custom provider type: {configuration.Class}.");

            customStatusProvider = (StatusProvider)Activator.CreateInstance(statusProviderType, configuration);
        }

        protected override ExternalUnit OnCheckStatus(ExternalUnit externalUnit)
        {
            return customStatusProvider.CheckStatus();
        }
    }
}