using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shelfly.Api.Constants;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates that the API process is alive and responsive.
/// </summary>
public class LivenessHealthCheck : IHealthCheck
{
    private static readonly ActivitySource Source = new(ActivitySources.HealthChecks);

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using Activity? activity = Source.StartActivity($"Liveness check: {context.Registration.Name}");
        activity?.SetTag(TagKeys.HealthCheck.Name, HealthCheckNames.Liveness);

        try
        {
            activity?.SetTag(TagKeys.HealthCheck.Status, HealthStatus.Healthy);

            HealthCheckResult result = HealthCheckResult.Healthy("Liveness check passed");
            activity?.SetStatus(ActivityStatusCode.Ok);

            return Task.FromResult(result);
        }
        catch (Exception exception)
        {
            HealthCheckResult exceptionResult = HealthCheckResult.Unhealthy(exception.Message);
            activity?.SetTag(TagKeys.HealthCheck.Status, HealthStatusValues.Unhealthy);
            activity?.SetTag(TagKeys.HealthCheck.Exception, exception.GetType().Name);
            activity?.SetStatus(ActivityStatusCode.Error);
            return Task.FromResult(exceptionResult);
        }
    }
}
