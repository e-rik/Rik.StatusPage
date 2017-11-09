using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    public class StatusPageConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("statusProviders", IsRequired = true)]
        public StatusProviderConfigurationCollection StatusProviders => (StatusProviderConfigurationCollection) this["statusProviders"];

        [ConfigurationProperty("application", IsRequired = true)]
        public ApplicationConfigurationElement Application => (ApplicationConfigurationElement) this["application"];
    }
}