using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates PostgreSQL database connectivity.
/// </summary>
public class PostgreSQLHealthCheck(string connectionString) : IHealthCheck
{
    private static readonly ActivitySource Source = new("shelfly-health-checks");

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using Activity? activity = Source.StartActivity($"PostgreSQL check: {context.Registration.Name}");
        activity?.SetTag("health.check.name", "postgresql");

        await using NpgsqlConnection connection = new NpgsqlConnection(connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            HealthCheckResult result = HealthCheckResult.Healthy("PostgreSQL is reachable");
            activity?.SetTag("health.check.status", "Healthy");
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            HealthCheckResult result = HealthCheckResult.Unhealthy(ex.Message);
            activity?.SetTag("health.check.status", "Unhealthy");
            activity?.SetTag("health.check.exception", ex.GetType().Name);
            activity?.SetStatus(ActivityStatusCode.Error);
            return result;
        }
    }
}
