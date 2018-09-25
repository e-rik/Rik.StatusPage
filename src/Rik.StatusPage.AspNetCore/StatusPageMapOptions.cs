using Microsoft.AspNetCore.Http;

namespace Rik.StatusPage.AspNetCore
{
    public class StatusPageMapOptions
    {
        public PathString Path { get; set; } = "/status.xml";
    }
}