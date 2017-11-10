using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    public class StatusProviderConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get => (string) this["name"];
            set => this["name"] = value;
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type => (string) this["type"];

        [ConfigurationProperty("connectionString", IsRequired = false)]
        public string ConnectionString
        {
            get => ((string)this["connectionString"]).GetAppSettingsOrValue();
            set => this["connectionString"] = value;
        }

        [ConfigurationProperty("storagePath", IsRequired = false)]
        public string StoragePath
        {
            get => ((string)this["storagePath"]).GetAppSettingsOrValue();
            set => this["storagePath"] = value;
        }

        [ConfigurationProperty("requireRead", IsRequired = false, DefaultValue = false)]
        public bool RequireRead
        {
            get => (bool)this["requireRead"];
            set => this["requireRead"] = value;
        }

        [ConfigurationProperty("requireWrite", IsRequired = false, DefaultValue = false)]
        public bool RequireWrite
        {
            get => (bool)this["requireWrite"];
            set => this["requireWrite"] = value;
        }

        [ConfigurationProperty("protocol", IsRequired = false)]
        public string Protocol
        {
            get => (string)this["protocol"];
            set => this["protocol"] = value;
        }

        [ConfigurationProperty("securityServer", IsRequired = false)]
        public string SecurityServer
        {
            get => ((string)this["securityServer"]).GetAppSettingsOrValue();
            set => this["securityServer"] = value;
        }

        [ConfigurationProperty("producerName", IsRequired = false)]
        public string ProducerName
        {
            get => ((string)this["producerName"]).GetAppSettingsOrValue();
            set => this["producerName"] = value;
        }

        [ConfigurationProperty("consumer", IsRequired = false)]
        public string Consumer
        {
            get => ((string)this["consumer"]).GetAppSettingsOrValue();
            set => this["consumer"] = value;
        }

        [ConfigurationProperty("userId", IsRequired = false)]
        public string UserId
        {
            get => ((string)this["userId"]).GetAppSettingsOrValue();
            set => this["userId"] = value;
        }

        [ConfigurationProperty("url", IsRequired = false)]
        public string Url
        {
            get => ((string)this["url"]).GetAppSettingsOrValue();
            set => this["url"] = value;
        }

        [ConfigurationProperty("class", IsRequired = false)]
        public string Class
        {
            get => ((string)this["class"]).GetAppSettingsOrValue();
            set => this["class"] = value;
        }
    }
}