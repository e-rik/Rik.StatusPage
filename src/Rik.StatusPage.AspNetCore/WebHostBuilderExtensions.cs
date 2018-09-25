using System;
using Microsoft.AspNetCore.Builder;

namespace Rik.StatusPage.AspNetCore
{
    public static class WebHostBuilderExtensions
    {
        public static IApplicationBuilder UseStatusPage<TStartup>(this IApplicationBuilder builder)
            where TStartup : class
        {
            return builder.UseStatusPage<TStartup>(new StatusPageOptions());
        }

        public static IApplicationBuilder UseStatusPage<TStartup>(this IApplicationBuilder builder, StatusPageOptions options)
            where TStartup : class
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            builder.IsolatedMap<TStartup>(options.Path);

            return builder;
        }
    }
}