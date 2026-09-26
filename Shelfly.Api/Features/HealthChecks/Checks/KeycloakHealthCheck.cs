using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Shelfly.Api.Constants;
using Shelfly.Configuration;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates Keycloak authentication service connectivity.
/// </summary>
public class KeycloakHealthCheck(IHttpClientFactory httpClientFactory, IOptionsMonitor<KeycloakConfig> keycloakOptionsMonitor) : IHealthCheck
{
    private static readonly ActivitySource Source = new(ActivitySources.HealthChecks);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using Activity? activity = Source.StartActivity($"Keycloak check: {context.Registration.Name}");
        activity?.SetTag(TagKeys.HealthCheck.Name, HealthCheckNames.Keycloak);

        try
        {
            KeycloakConfig keycloakConfig = keycloakOptionsMonitor.CurrentValue;
            string realmEndpoint = $"/realms/{keycloakConfig.Realm}";
            using HttpClient httpClient = httpClientFactory.CreateClient("Keycloak");
            HttpResponseMessage response = await httpClient.GetAsync(realmEndpoint, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                HealthCheckResult result = HealthCheckResult.Healthy("Keycloak is reachable");
                activity?.SetTag(TagKeys.HealthCheck.Status, HealthStatusValues.Healthy);
                activity?.SetStatus(ActivityStatusCode.Ok);
                return result;
            }

            HealthCheckResult unhealthyResult = HealthCheckResult.Unhealthy($"Keycloak returned {(int)response.StatusCode}");
            activity?.SetTag(TagKeys.HealthCheck.Status, HealthStatusValues.Unhealthy);
            activity?.SetTag(TagKeys.HealthCheck.ResponseStatusCode, (int)response.StatusCode);
            activity?.SetStatus(ActivityStatusCode.Error);
            return unhealthyResult;
        }
        catch (Exception ex)
        {
            HealthCheckResult exceptionResult = HealthCheckResult.Unhealthy(ex.Message);
            activity?.SetTag(TagKeys.HealthCheck.Status, HealthStatusValues.Unhealthy);
            activity?.SetTag(TagKeys.HealthCheck.Exception, ex.GetType().Name);
            activity?.SetStatus(ActivityStatusCode.Error);
            return exceptionResult;
        }
    }
}
