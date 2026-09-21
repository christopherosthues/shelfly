using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates Keycloak authentication service connectivity.
/// </summary>
public class KeycloakHealthCheck(HttpClient httpClient, string realm) : IHealthCheck
{
    private readonly string _realmEndpoint = $"/realms/{realm}";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(_realmEndpoint, cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Keycloak is reachable")
                : HealthCheckResult.Unhealthy($"Keycloak returned {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
