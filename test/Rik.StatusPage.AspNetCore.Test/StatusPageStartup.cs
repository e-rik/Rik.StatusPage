using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Rik.StatusPage.AspNetCore.Test
{
    public class StatusPageStartup
    {
        private readonly IConfiguration configuration;

        public StatusPageStartup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStatusPage(configuration);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<StatusPageMiddleware>();
        }
    }
}