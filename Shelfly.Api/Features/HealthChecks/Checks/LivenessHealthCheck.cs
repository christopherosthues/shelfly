using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Basic process liveness health check — verifies the API process is running.
/// </summary>
public class LivenessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(HealthCheckResult.Healthy("Process is running"));
}
