using Microsoft.AspNetCore.Http;

namespace Rik.StatusPage.AspNetCore
{
    public class StatusPageOptions
    {
        public PathString Path { get; set; } = "/status.xml";
    }
}