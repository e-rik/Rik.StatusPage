using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Rik.StatusPage.AspNetCore.Test
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            var statusPageStartUp = new StatusPageStartup(app.ApplicationServices.GetRequiredService<IConfiguration>());

            app.UseStatusPage(statusPageStartUp.Configure, _ => statusPageStartUp.ConfigureServices());

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}