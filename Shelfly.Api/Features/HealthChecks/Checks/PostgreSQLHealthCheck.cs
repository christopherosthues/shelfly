using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates PostgreSQL database connectivity.
/// </summary>
public class PostgreSQLHealthCheck(string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL is reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
