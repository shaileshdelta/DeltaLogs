using DeltaLogs.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DeltaLogs.Extensions
{
    public static class DeltaLoggerExtensions
    {
        public static IServiceCollection AddDeltaLogger(this IServiceCollection services)
        {
            services.AddControllers().AddApplicationPart(typeof(Controllers.LoggerController).Assembly);
            return services;
        }

        public static IApplicationBuilder UseDeltaLogger(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
