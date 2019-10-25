using System;
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

        public IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddStatusPage(configuration);

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStatusPageMiddleware();
        }
    }
}