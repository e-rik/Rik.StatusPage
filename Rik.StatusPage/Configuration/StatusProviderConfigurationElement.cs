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
                var rawValue = (string) this["connectionString"];

                if (rawValue.StartsWith("${") && rawValue.EndsWith("}"))
                {
                    var connectionString = rawValue.Substring(2, rawValue.Length - 3);
                    return ConfigurationManager.AppSettings[connectionString];
                }

                return rawValue;
            }
        }

        [ConfigurationProperty("storagePath", IsRequired = false)]
        public string StoragePath
        {
            get
            {
                var rawValue = (string) this["storagePath"];

                if (rawValue.StartsWith("${") && rawValue.EndsWith("}"))
                {
                    var storagePath = rawValue.Substring(2, rawValue.Length - 3);
                    return ConfigurationManager.AppSettings[storagePath];
                }

                return rawValue;
            }
        }

        [ConfigurationProperty("requireRead", IsRequired = false, DefaultValue = false)]
        public bool RequireRead => (bool) this["requireRead"];

        [ConfigurationProperty("requireWrite", IsRequired = false, DefaultValue = false)]
        public bool RequireWrite => (bool) this["requireWrite"];
    }
}