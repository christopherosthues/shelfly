using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates that the API process is alive and responsive.
/// </summary>
public class LivenessHealthCheck : IHealthCheck
{
    private static readonly ActivitySource Source = new("shelfly-health-checks");

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var activity = Source.StartActivity($"Liveness check: {context.Registration.Name}");
        activity?.SetTag("health.check.name", "liveness");
        activity?.SetTag("health.check.status", "Healthy");

        var result = HealthCheckResult.Healthy("Liveness check passed");
        activity?.SetStatus(ActivityStatusCode.Ok);

        return result;
    }
}
