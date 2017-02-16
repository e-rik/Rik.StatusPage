using System;
using Rik.StatusPage.Configuration;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.Providers
{
    public abstract class StatusProvider
    {
        protected readonly StatusProviderConfigurationElement configuration;

        protected StatusProvider(StatusProviderConfigurationElement configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrWhiteSpace(configuration.Name))
                throw new ArgumentException("Status provider name is required.", nameof(configuration.Name));

            this.configuration = configuration;
        }

        protected abstract ExternalUnit OnCheckStatus(ExternalUnit externalUnit);

        public virtual ExternalUnit CheckStatus()
        {
            var externalUnit = new ExternalUnit { Name = configuration.Name };

            try
            {
                return OnCheckStatus(externalUnit);
            }
            catch (Exception exception)
            {
                return externalUnit.SetStatus(UnitStatus.NotOk, exception.Message);
            }
        }
    }
}