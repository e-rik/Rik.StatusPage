using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.Extensions.DependencyInjection;
using Rik.StatusPage.Providers;
using Rik.StatusPage.Schema;

namespace Rik.StatusPage.AspNetCore
{
    public static class ApplicationBuilderExtensions
    {
        private static readonly Func<IList<IStatusProvider>, CancellationToken, Task> defaultStatusProviderCustomization = (_, __) => Task.CompletedTask;
        private static readonly Func<Application, CancellationToken, Task> defaultResultCustomization = (_, __) => Task.CompletedTask;

        public static IApplicationBuilder UseStatusPageMiddleware(this IApplicationBuilder app, Func<IList<IStatusProvider>, CancellationToken, Task> statusProviderCustomization = null, Func<Application, CancellationToken, Task> resultCustomization = null) =>
            app.UseMiddleware<StatusPageMiddleware>(statusProviderCustomization ?? defaultStatusProviderCustomization, resultCustomization ?? defaultResultCustomization);

        public static IApplicationBuilder UseStatusPage(this IApplicationBuilder app, Action<IApplicationBuilder> configuration, Func<IServiceProvider, IServiceProvider> registration) =>
            app.UseStatusPage(new StatusPageMapOptions(), configuration, registration);

        public static IApplicationBuilder UseStatusPage(this IApplicationBuilder app, StatusPageMapOptions options, Action<IApplicationBuilder> configuration, Func<IServiceProvider, IServiceProvider> registration)
        {
            var provider = registration(app.ApplicationServices);

            var builder = new ApplicationBuilder(null) { ApplicationServices = provider };

            builder.Use(async (context, next) =>
            {
                var factory = provider.GetRequiredService<IServiceScopeFactory>();

                context.Items[typeof(IServiceProvider)] = context.RequestServices;

                try
                {
                    using var scope = factory.CreateScope();
                    context.RequestServices = scope.ServiceProvider;
                    await next();
                }

                finally
                {
                    context.RequestServices = null;
                }
            });

            builder.Map(options.Path, configuration);

            return app.Use(next =>
            {
                builder.Run(async context =>
                {
                    var factory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

                    try
                    {
                        using var scope = factory.CreateScope();
                        context.RequestServices = scope.ServiceProvider;
                        await next(context);
                    }

                    finally
                    {
                        context.RequestServices = null;
                    }
                });

                var branch = builder.Build();

                return context => branch(context);
            });
        }
    }
}