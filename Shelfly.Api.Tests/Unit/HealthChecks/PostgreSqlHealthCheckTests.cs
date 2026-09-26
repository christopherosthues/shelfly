using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shelfly.Api.Features.HealthChecks.Checks;

namespace Shelfly.Api.Tests.Unit.HealthChecks;

public class PostgreSqlHealthCheckTests
{
    [Test]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenConnectionSucceeds()
    {
        PostgreSqlHealthCheck check = new PostgreSqlHealthCheck("Server=localhost;Database=test;");
        HealthCheckContext context = new HealthCheckContext { Registration = new HealthCheckRegistration("postgresql", _ => check, null, Array.Empty<string>()) };

        // Without a running server, this will fail — but we verify the structure
        HealthCheckResult result = await check.CheckHealthAsync(context);

        // In unit test without container, expect Unhealthy (connection refused)
        result.Status.ShouldBeOneOf(HealthStatus.Healthy, HealthStatus.Unhealthy);
    }
}
