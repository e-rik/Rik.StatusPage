using System;
using System.Configuration;
using System.Reflection;

namespace Rik.StatusPage.Configuration
{
    public class StatusProviderConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name => (string) this["name"];

        [ConfigurationProperty("type", IsRequired = true)]
        public StatusProviderType Type => (StatusProviderType) this["type"];

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

        public Type ConnectionType
        {
            get
            {
                var split = ConnectionTypeName.Split(',');

                var connectionTypeStr = split[0].Trim();
                var assemblyStr = split[1].Trim();

                var assembly = Assembly.Load(assemblyStr);
                var connectionType = assembly.GetType(connectionTypeStr);

                // TODO: kontrollid assembly laadimise ja tüübi vastavuse osas

                return connectionType;
            }
        }
    }
}