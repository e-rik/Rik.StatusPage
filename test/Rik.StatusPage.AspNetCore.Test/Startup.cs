using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Rik.StatusPage.AspNetCore.Test
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseStatusPage<StatusPageStartup>();

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}