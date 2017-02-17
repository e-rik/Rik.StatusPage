using System.Configuration;

namespace Rik.StatusPage.Configuration
{
    public class StatusProviderConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }

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
            set { this["storagePath"] = value; }
        }

        [ConfigurationProperty("requireRead", IsRequired = false, DefaultValue = false)]
        public bool RequireRead
        {
            get { return (bool) this["requireRead"]; }
            set { this["requireRead"] = value; }
        }

        [ConfigurationProperty("requireWrite", IsRequired = false, DefaultValue = false)]
        public bool RequireWrite
        {
            get { return (bool) this["requireWrite"]; }
            set { this["requireWrite"] = value; }
        }

        [ConfigurationProperty("protocol", IsRequired = false)]
        public string Protocol
        {
            get { return (string) this["protocol"]; }
            set { this["protocol"] = value; }
        }

        [ConfigurationProperty("securityServer", IsRequired = false)]
        public string SecurityServer
        {
            get
            {
                var rawValue = (string)this["securityServer"];

                if (rawValue.StartsWith("${") && rawValue.EndsWith("}"))
                {
                    var storagePath = rawValue.Substring(2, rawValue.Length - 3);
                    return ConfigurationManager.AppSettings[storagePath];
                }

                return rawValue;
            }
            set { this["securityServer"] = value; }
        }

        [ConfigurationProperty("producerName", IsRequired = false)]
        public string ProducerName
        {
            get
            {
                var rawValue = (string)this["producerName"];

                if (rawValue.StartsWith("${") && rawValue.EndsWith("}"))
                {
                    var storagePath = rawValue.Substring(2, rawValue.Length - 3);
                    return ConfigurationManager.AppSettings[storagePath];
                }

                return rawValue;
            }
            set { this["producerName"] = value; }
        }

        [ConfigurationProperty("consumer", IsRequired = false)]
        public string Consumer
        {
            get
            {
                var rawValue = (string)this["consumer"];

                if (rawValue.StartsWith("${") && rawValue.EndsWith("}"))
                {
                    var storagePath = rawValue.Substring(2, rawValue.Length - 3);
                    return ConfigurationManager.AppSettings[storagePath];
                }

                return rawValue;
            }
            set { this["consumer"] = value; }
        }

        [ConfigurationProperty("userId", IsRequired = false)]
        public string UserId
        {
            get
            {
                var rawValue = (string)this["userId"];

                if (rawValue.StartsWith("${") && rawValue.EndsWith("}"))
                {
                    var storagePath = rawValue.Substring(2, rawValue.Length - 3);
                    return ConfigurationManager.AppSettings[storagePath];
                }

                return rawValue;
            }
            set { this["userId"] = value; }
        }
    }
}