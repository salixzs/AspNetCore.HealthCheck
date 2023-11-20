using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Salix.AspNetCore.HealthCheck;

/// <summary>
/// Custom formatter for HealthCheck output.
/// </summary>
public static class HealthCheckFormatter
{
    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>
    /// Transforms HealthCheck report into customized JSON string with all included information.
    /// </summary>
    /// <param name="context">The HTTP context where response needs to be written to.</param>
    /// <param name="report">The Health report itself.</param>
    /// <param name="isDevelopment">
    /// When true, might include information, which is dangerous for public use, but valuable in development/testing.
    /// When false - will not show added Data, Stack trace etc.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is <c>null</c>.</exception>
    public static Task JsonResponseWriter(HttpContext context, HealthReport report, bool isDevelopment = false)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        string result = "{}";
        if (report != null)
        {
            result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    key = e.Key,
                    status = Enum.GetName(typeof(HealthStatus), e.Value.Status),
                    description = e.Value.Description,
                    exception = GetExceptionObject(e.Value.Exception, isDevelopment),
                    data = e.Value.Data.Select(d => new
                    {
                        key = d.Key,
                        value = d.Value.ToString(),
                    }),
                }),
            },
            JsonSerializerOptions);
        }

        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(result);
    }

    /// <summary>
    /// Pre-formats Exception object to simpler form, leaving only necessary information.
    /// For Non-DEV environments will not add stacktrace and data dictionary.
    /// </summary>
    /// <param name="exception">Exception to simplify.</param>
    /// <param name="showAllInformation">In DEV this should be true to add stacktrace and data dictionary.</param>
    private static object? GetExceptionObject(Exception? exception, bool showAllInformation)
    {
        if (exception == null)
        {
            return null;
        }

        return new
        {
            message = exception.Message,
            type = exception.GetType().Name,
            innerException = exception.InnerException?.Message,
            stackTrace = GetStackTrace(exception, showAllInformation),
            data = showAllInformation ? exception.Data : null,
        };
    }

    /// <summary>
    /// Simplifies stack trace or omits it altogether when not DEV.
    /// </summary>
    /// <param name="exception">Exception with Stack Trace information.</param>
    /// <param name="showStackTrace">Should we [false] skip or [true] - show this.</param>
    private static string? GetStackTrace(Exception? exception, bool showStackTrace)
    {
        if (exception == null || exception.StackTrace == null || !showStackTrace)
        {
            return null;
        }

        string[] stackTraceFrames = exception.StackTrace.Split(new string[] { "[\r\n]" }, StringSplitOptions.RemoveEmptyEntries);
        var resultingStackTrace = new StringBuilder();
        foreach (string frame in stackTraceFrames)
        {
            if (frame.Contains(":line", StringComparison.InvariantCultureIgnoreCase))
            {
                resultingStackTrace.AppendLine(frame);
            }
        }

        return resultingStackTrace.ToString();
    }
}
