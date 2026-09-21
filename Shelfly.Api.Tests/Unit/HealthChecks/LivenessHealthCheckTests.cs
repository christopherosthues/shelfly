using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shelfly.Api.Features.HealthChecks.Checks;

namespace Shelfly.Api.Tests.Unit.HealthChecks;

public class LivenessHealthCheckTests
{
    [Test]
    public async Task CheckHealthAsync_ReturnsHealthy()
    {
        LivenessHealthCheck check = new LivenessHealthCheck();
        HealthCheckContext context = new HealthCheckContext { Registration = new HealthCheckRegistration("liveness", _ => check, null, Array.Empty<string>()) };

        HealthCheckResult result = await check.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Healthy);
    }
}
