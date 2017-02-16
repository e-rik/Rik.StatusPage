using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    public class StatusPageConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("statusProviders", IsRequired = false)]
        [ConfigurationCollection(typeof(StatusProviderConfigurationCollection), AddItemName = "statusProvider")]
        public StatusProviderConfigurationCollection StatusProviders => this["statusProviders"] as StatusProviderConfigurationCollection;
    }
}