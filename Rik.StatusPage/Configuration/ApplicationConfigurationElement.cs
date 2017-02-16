using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    public class ApplicationConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name => this["name"] as string;

        [ConfigurationProperty("version", IsRequired = true)]
        public string Version => this["version"] as string;
    }
}