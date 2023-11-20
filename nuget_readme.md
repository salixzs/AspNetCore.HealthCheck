# Salix.AspNetCore.HealthCheck

Package provides two functionalities for AspNet (Core) APIs:
- Custom formatter of health check results as JSON object (default response is plain-text)
- Page, displaying health check results in humanly readable format in colors.

## JSON formatter

Returns all defined health checks as JSOn object with implemented details and error handling (exception details).\
Example response:

```json
{
    "status": "Healthy",
    "checks": [
        {
            "key": "Database",
            "status": "Healthy",
            "description": "Database is OK.",
            "exception": null,
            "data": [
                {
                    "key": "ConnString",
                    "value": "Connection string (shown only in developer mode)"
                }
            ]
        },
        {
            "key": "ExtApi",
            "status": "Healthy",
            "description": "ExtAPI is OK.",
            "exception": null,
            "data": [
                {
                    "key": "ExtApi URL",
                    "value": "https://extapi.com/api"
                },
                {
                    "key": "User",
                    "value": "username from config"
                },
                {
                    "key": "Password",
                    "value": "password from config"
                },
                {
                    "key": "Token",
                    "value": "Secret token from config"
                }
            ]
        }
    ]
}
```

## Health check page

Controller action content compiler to issue HTML page with health check results:
![Health check page](./DocImages/health-check-page.JPG)

Additional possibility to add some custom links to implemented functionalities for some sandbox or testing links (or anything else) to thi page.\
**NOTE:** Does NOT bring entire MVC stack to display page.

## Usage

Register your health checks normally as described in [official Microsoft documentation](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-7.0):

```csharp
builder.Services.AddHealthChecks()
    .Add(new HealthCheckRegistration("Database", sp => new DummyDatabaseHealthCheck(builder.Environment.IsDevelopment()), HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(10)))
    .Add(new HealthCheckRegistration("ExtApi", sp => new DummyExternalApiHealthCheck(builder.Environment.IsDevelopment()), HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(5)));
```

Then for WebApplication builder, use provided extension to use JSON fomatter:

```csharp
app.UseJsonHealthChecks("/health", builder.Environment.IsDevelopment());
```

### Health page

To add health page (humanized view of health check results), create or use existing controller and add action endpoint like in sample below:

```csharp
[ApiController]
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
                }),
        };
    }
}
```
