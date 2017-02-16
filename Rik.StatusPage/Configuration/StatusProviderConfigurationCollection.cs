using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    public class StatusProviderConfigurationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new StatusProviderConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StatusProviderConfigurationElement) element).Name;
        }
    }
}