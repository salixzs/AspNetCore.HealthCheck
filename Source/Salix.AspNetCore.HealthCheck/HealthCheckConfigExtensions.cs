using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;

namespace Salix.AspNetCore.HealthCheck;

/// <summary>
/// Registers Helth checks with Json Response formatter.
/// </summary>
public static class HealthCheckConfigExtensions
{
    /// <summary>
    /// Registers Health checks with Json response formatter.
    /// <code>
    /// app.UseJsonHealthChecks("/api/health", builder.Environment.IsDevelopment());
    /// </code>
    /// </summary>
    /// <param name="app">Application object builder.</param>
    /// <param name="path">
    /// Relative Path where Healt result is provided.
    /// <code>
    /// app.UseJsonHealthChecks("/api/health");
    /// </code>
    /// </param>
    /// <param name="isDevelopment">
    /// Controls some data to be hidden when this is false (non-deveopment environment: production).
    /// <code>
    /// app.UseJsonHealthChecks("/api/health", builder.Environment.IsDevelopment());
    /// </code>
    /// </param>
    public static IApplicationBuilder UseJsonHealthChecks(this IApplicationBuilder app, PathString path, bool isDevelopment = false)
    {
        var healthCheckOptions = new HealthCheckOptions
        {
            ResponseWriter = async (context, report) => await HealthCheckFormatter.JsonResponseWriter(context, report, isDevelopment).ConfigureAwait(false),
        };

        app.UseHealthChecks(path, healthCheckOptions);
        return app;
    }
}
