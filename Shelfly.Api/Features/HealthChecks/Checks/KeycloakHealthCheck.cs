using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates Keycloak authentication service connectivity.
/// </summary>
public class KeycloakHealthCheck(HttpClient httpClient, string realm) : IHealthCheck
{
    private static readonly ActivitySource Source = new("shelfly-health-checks");
    private readonly string _realmEndpoint = $"/realms/{realm}";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using Activity? activity = Source.StartActivity($"Keycloak check: {context.Registration.Name}");
        activity?.SetTag("health.check.name", "keycloak");

        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(_realmEndpoint, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                HealthCheckResult result = HealthCheckResult.Healthy("Keycloak is reachable");
                activity?.SetTag("health.check.status", "Healthy");
                activity?.SetStatus(ActivityStatusCode.Ok);
                return result;
            }

            HealthCheckResult unhealthyResult = HealthCheckResult.Unhealthy($"Keycloak returned {(int)response.StatusCode}");
            activity?.SetTag("health.check.status", "Unhealthy");
            activity?.SetTag("http.response_status_code", (int)response.StatusCode);
            activity?.SetStatus(ActivityStatusCode.Error);
            return unhealthyResult;
        }
        catch (Exception ex)
        {
            HealthCheckResult exceptionResult = HealthCheckResult.Unhealthy(ex.Message);
            activity?.SetTag("health.check.status", "Unhealthy");
            activity?.SetTag("health.check.exception", ex.GetType().Name);
            activity?.SetStatus(ActivityStatusCode.Error);
            return exceptionResult;
        }
    }
}
