using System.Collections.Generic;

namespace Rik.StatusPage.Configuration
{
    public class CustomStatusProviderOptions : StatusProviderOptions
    {
        public string Class { get; set; }
        public IDictionary<string, string> OtherOptions { get; set; }
    }
}