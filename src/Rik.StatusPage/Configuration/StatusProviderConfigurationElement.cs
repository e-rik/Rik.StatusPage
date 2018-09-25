using System.Collections.Generic;
using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    public class StatusProviderConfigurationElement : ConfigurationElement
    {
        public readonly IDictionary<string, string> UnrecognizedAttributes = new Dictionary<string, string>();

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name { get => (string) this["name"]; set => this["name"] = value; }

        [ConfigurationProperty("provider", IsRequired = true)]
        public string Provider => (string)this["provider"];

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            UnrecognizedAttributes.Add(name, value);
            return true;
        }
    }
}