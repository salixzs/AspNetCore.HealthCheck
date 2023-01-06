using Api.HealthCheck.Net7.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Salix.AspNetCore.HealthCheck;

namespace Api.HealthCheck.Net7;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        // -----> Here normally add separate health checks for components
        builder.Services.AddHealthChecks()
            .Add(new HealthCheckRegistration("Database", sp => new DummyDatabaseHealthCheck(builder.Environment.IsDevelopment()), HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(10)))
            .Add(new HealthCheckRegistration("ExtApi", sp => new DummyExternalApiHealthCheck(builder.Environment.IsDevelopment()), HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(5)));

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        // -----> This specifies Health Check usage with JSON reponse.
        app.UseJsonHealthChecks("/health", builder.Environment.IsDevelopment());

        app.Run();
    }
}
