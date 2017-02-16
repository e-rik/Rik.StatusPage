using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    public class StatusProviderConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name => (string) this["name"];

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type => (string) this["type"];

        [ConfigurationProperty("connectionString", IsRequired = false)]
        public string ConnectionString
        {
            get
            {
                var connectionStringRaw = (string) this["connectionString"];

                if (connectionStringRaw.StartsWith("${") && connectionStringRaw.EndsWith("}"))
                {
                    var connectionString = connectionStringRaw.Substring(2, connectionStringRaw.Length - 3);
                    return ConfigurationManager.AppSettings[connectionString];
                }

                return connectionStringRaw;
            }
        }

        [ConfigurationProperty("connectionType", IsRequired = false)]
        public string ConnectionTypeName => (string) this["connectionType"];
    }
}