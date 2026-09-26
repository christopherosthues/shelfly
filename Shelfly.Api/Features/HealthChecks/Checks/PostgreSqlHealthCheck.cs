using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;
using Shelfly.Api.Constants;
using Shelfly.Api.Extensions;
using Shelfly.Configuration;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates PostgreSQL database connectivity.
/// </summary>
public class PostgreSqlHealthCheck(IOptionsMonitor<ServerDynamicConfiguration> serverDynamicConfigurationOptionsMonitor, IConfiguration configuration) : IHealthCheck
{
    private static readonly ActivitySource Source = new(ActivitySources.HealthChecks);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using Activity? activity = Source.StartActivity($"PostgreSQL check: {context.Registration.Name}");
        activity?.SetTag(TagKeys.HealthCheck.Name, HealthCheckNames.PostgreSql);

        try
        {
            PostgreSqlConfig postgreSqlConfig = serverDynamicConfigurationOptionsMonitor.CurrentValue.PostgreSql;
            string connectionString = PostgreSqlOptionsExtensions.BuildPostgresConnectionString(postgreSqlConfig, configuration);
            await using NpgsqlConnection connection = new(connectionString);

            await connection.OpenAsync(cancellationToken);
            HealthCheckResult result = HealthCheckResult.Healthy("PostgreSQL is reachable");
            activity?.SetTag(TagKeys.HealthCheck.Status, HealthStatusValues.Healthy);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            HealthCheckResult result = HealthCheckResult.Unhealthy(ex.Message);
            activity?.SetTag(TagKeys.HealthCheck.Status, HealthStatusValues.Unhealthy);
            activity?.SetTag(TagKeys.HealthCheck.Exception, ex.GetType().Name);
            activity?.SetStatus(ActivityStatusCode.Error);
            return result;
        }
    }
}
