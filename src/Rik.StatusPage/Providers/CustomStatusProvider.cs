using System;
using System.Linq;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public class CustomStatusProvider : StatusProvider
    {
        private readonly Type statusProviderType;

        public CustomStatusProvider(StatusProviderConfigurationElement configuration)
            : base(configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.Class))
                throw new ArgumentException("Class name is required for custom provider.", nameof(configuration.Class));

            statusProviderType = FindType(configuration.Class);

            if (statusProviderType == null)
                throw new Exception($"Cannot load custom provider type: {configuration.Class}.");
        }

        protected override ExternalUnit OnCheckStatus(ExternalUnit externalUnit)
        {
            var customProvider = (StatusProvider) Activator.CreateInstance(statusProviderType, configuration);

            return customProvider.CheckStatus();
        }
    }
}