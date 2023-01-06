using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Salix.AspNetCore.HealthCheck;

namespace Api.HealthCheck.Net7.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly HealthCheckService _healthChecks;

    public HealthCheckController(HealthCheckService healthChecks) =>
        _healthChecks = healthChecks;

    [HttpGet("/healthpage")]
    public async Task<ContentResult> ShowHealth()
    {
        var healthResult = await _healthChecks.CheckHealthAsync();
        return new ContentResult
        {
            ContentType = "text/html",
            StatusCode = (int)HttpStatusCode.OK,
            Content = HealthTestPage.GetContents(
                healthReport: healthResult,
                originalHealthTestEndpoint: "/health",
                testingLinks: new List<HealthTestPageLink>
                {
                        new HealthTestPageLink { TestEndpoint = "/api/sample/exception", Name = "Exception", Description = "Throws dummy exception/error to check Json Error functionality." },
                        new HealthTestPageLink { TestEndpoint = "/api/sample/validation", Name = "Validation Error", Description = "Throws dummy data validation exception to check Json Error functionality for data validation." },
                        new HealthTestPageLink { TestEndpoint = "/api/sample/db", Name = "DB Exception", Description = "Throws dummy database exception to check Json Error custom exception handling." },
                        new HealthTestPageLink { TestEndpoint = "/api/sample/canceled", Name = "Operation canceled Exception", Description = "Mimics OperationCanceledException thrown by some dependencies in async operations via CancellationToken. Swallows error!" },
                        new HealthTestPageLink { TestEndpoint = "/api/sample/notyet", Name = "Not yet!", Description = "Showcase Json handler work on NotImplementedException." },
                        new HealthTestPageLink { TestEndpoint = "/api/sample/anytest", Name = "DateTime", Description = "Showcase some custom API testing endpont returning some data." },
                }),
        };
    }
}
