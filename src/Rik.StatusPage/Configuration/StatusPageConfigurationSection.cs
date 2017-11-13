using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    public class StatusPageConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("externalDependencies", IsRequired = false)]
        public StatusProviderConfigurationCollection StatusProviders => (StatusProviderConfigurationCollection) this["externalDependencies"];

        [ConfigurationProperty("app", IsRequired = false)]
        public ApplicationConfigurationElement Application => (ApplicationConfigurationElement)this["app"];
    }
}