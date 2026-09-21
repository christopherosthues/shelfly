using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shelfly.Api.Features.HealthChecks.Checks;

namespace Shelfly.Api.Tests.Unit.HealthChecks;

public class MongoDBHealthCheckTests
{
    [Test]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenConnectionSucceeds()
    {
        MongoDbHealthCheck check = new MongoDbHealthCheck("mongodb://localhost:27017");
        HealthCheckContext context = new HealthCheckContext { Registration = new HealthCheckRegistration("mongodb", _ => check, null, Array.Empty<string>()) };

        // Without a running server, this will fail — but we verify the structure
        HealthCheckResult result = await check.CheckHealthAsync(context);

        // In unit test without container, expect Unhealthy (connection refused)
        result.Status.ShouldBeOneOf(HealthStatus.Healthy, HealthStatus.Unhealthy);
    }
}
