using System.Configuration;
using System.Text.RegularExpressions;

namespace Rik.StatusPage.Configuration
{
    public static class ConfigurationExtensions
    {
        private static readonly Regex appSettingsReferencePattern = new Regex("\\${(.*?)}");

        public static string GetAppSettingsOrValue(this string configurationValue)
        {
            var match = appSettingsReferencePattern.Match(configurationValue);

            if (!match.Success)
                return configurationValue;

            var connectionString = match.Groups[1].Value;
            return ConfigurationManager.AppSettings[connectionString];
        }
    }
}