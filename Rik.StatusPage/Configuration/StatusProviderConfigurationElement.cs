using System.Configuration;
using System.Text.RegularExpressions;

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
                var match = Regex.Match(rawValue, "\\${(.*?)}");

                if (!match.Success)
                    return rawValue;

                var connectionString = match.Groups[1].Value;
                return ConfigurationManager.AppSettings[connectionString];
            }
        }

        [ConfigurationProperty("storagePath", IsRequired = false)]
        public string StoragePath
        {
            get
            {
                var rawValue = (string) this["storagePath"];
                var match = Regex.Match(rawValue, "\\${(.*?)}");

                if (!match.Success)
                    return rawValue;

                var storagePath = match.Groups[1].Value;
                return ConfigurationManager.AppSettings[storagePath];
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
                var match = Regex.Match(rawValue, "\\${(.*?)}");

                if (!match.Success)
                    return rawValue;

                var securityServer = match.Groups[1].Value;
                return ConfigurationManager.AppSettings[securityServer];
            }
            set { this["securityServer"] = value; }
        }

        [ConfigurationProperty("producerName", IsRequired = false)]
        public string ProducerName
        {
            get
            {
                var rawValue = (string)this["producerName"];
                var match = Regex.Match(rawValue, "\\${(.*?)}");

                if (!match.Success)
                    return rawValue;

                var producerName = match.Groups[1].Value;
                return ConfigurationManager.AppSettings[producerName];
            }
            set { this["producerName"] = value; }
        }

        [ConfigurationProperty("consumer", IsRequired = false)]
        public string Consumer
        {
            get
            {
                var rawValue = (string)this["consumer"];
                var match = Regex.Match(rawValue, "\\${(.*?)}");

                if (!match.Success)
                    return rawValue;

                var consumer = match.Groups[1].Value;
                return ConfigurationManager.AppSettings[consumer];
            }
            set { this["consumer"] = value; }
        }

        [ConfigurationProperty("userId", IsRequired = false)]
        public string UserId
        {
            get
            {
                var rawValue = (string)this["userId"];
                var match = Regex.Match(rawValue, "\\${(.*?)}");

                if (!match.Success)
                    return rawValue;

                var userId = match.Groups[1].Value;
                return ConfigurationManager.AppSettings[userId];
            }
            set { this["userId"] = value; }
        }

        [ConfigurationProperty("url", IsRequired = false)]
        public string Url
        {
            get
            {
                var rawValue = (string)this["url"];
                var match = Regex.Match(rawValue, "\\${(.*?)}");

                if (!match.Success)
                    return rawValue;

                var url = match.Groups[1].Value;
                return ConfigurationManager.AppSettings[url];
            }
            set { this["url"] = value; }
        }
    }
}