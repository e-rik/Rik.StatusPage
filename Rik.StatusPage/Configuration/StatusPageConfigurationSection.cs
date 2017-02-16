using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    public class StatusPageConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("statusProviders", IsRequired = true)]
        [ConfigurationCollection(typeof(StatusProviderConfigurationCollection), AddItemName = "statusProvider")]
        public StatusProviderConfigurationCollection StatusProviders => this["statusProviders"] as StatusProviderConfigurationCollection;
    }
}