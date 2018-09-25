using System.Collections.Generic;

namespace Rik.StatusPage.AspNetCore
{
    public class StatusPageOptions
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Version { get; set; }
        public IDictionary<string, string> AdditionalInfo { get; set; }
    }
}