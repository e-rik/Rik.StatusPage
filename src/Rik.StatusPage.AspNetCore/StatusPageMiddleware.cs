using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Rik.StatusPage.AspNetCore
{
    public class StatusPageMiddleware
    {
        private readonly RequestDelegate next;

        public StatusPageMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task InvokeAsync(HttpContext httpContext)
        {
            return Task.CompletedTask;
        }
    }
}