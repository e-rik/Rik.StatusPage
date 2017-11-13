using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    [ConfigurationCollection(typeof(StatusProviderConfigurationCollection), AddItemName = "unit")]
    public class StatusProviderConfigurationCollection : ConfigurationElementCollection
    {
        public StatusProviderConfigurationElement this[int index]
        {
            get => BaseGet(index) as StatusProviderConfigurationElement;
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);

                BaseAdd(index, value);
            }
        }

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