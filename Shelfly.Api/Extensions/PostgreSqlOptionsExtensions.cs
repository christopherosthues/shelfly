using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Configuration;

namespace Shelfly.Api.Extensions;

/// <summary>
/// Extension methods for building PostgreSQL connection strings and registering EF Core.
/// </summary>
public static class PostgreSqlOptionsExtensions
{
    /// <summary>
    /// Composes a PostgreSQL connection string from individual configuration parts stored in MongoDB,
    /// with fallback to vault secret file for the password. Falls back to ConnectionStrings:PostgreSql
    /// if the individual config keys are not present (e.g., during development or build).
    /// </summary>
    public static string BuildPostgresConnectionString(PostgreSqlConfig? postgreSqlConfig, IConfigurationRoot config)
    {
        // Fallback: use pre-formed connection string from environment variable for development
        string? fallback = config.GetConnectionString("PostgreSql");
        if (!string.IsNullOrEmpty(fallback))
        {
            return fallback;
        }

        string host = postgreSqlConfig?.Host ?? config.GetValue<string>("PostgreSql:Host")
                        ?? throw new InvalidOperationException("PostgreSql:Host not configured");

        int port = postgreSqlConfig?.Port ?? config.GetValue<int?>("PostgreSql:Port") ?? 5432;

        string username = postgreSqlConfig?.Username ?? config.GetValue<string>("PostgreSql:Username")
                          ?? throw new InvalidOperationException("PostgreSql:Username not configured");

        string password = config.GetValue<string>("PostgreSql:Password")
                          ?? ReadSecretFile(config.GetValue<string?>("POSTGRESQL_PASSWORD_FILE"))
                          ?? throw new InvalidOperationException("PostgreSql:Password not configured");

        string database = postgreSqlConfig?.Database ?? config.GetValue<string>("PostgreSql:Database")
                         ?? "shelfly";

        return $"Host={host};Port={port};Username={username};Password={password};Database={database}";
    }

    /// <summary>
    /// Reads a secret value from a file path (Docker Compose secrets mount point or HashiCorp Vault).
    /// </summary>
    private static string? ReadSecretFile(string? filePath) =>
        filePath is not null && File.Exists(filePath) ? File.ReadAllText(filePath).Trim() : null;

    /// <summary>
    /// Registers EF Core with PostgreSQL using the connection string built from MongoDB config.
    /// </summary>
    public static IServiceCollection AddShelflyDbContext(this IServiceCollection services, IConfigurationRoot config, PostgreSqlConfig? postgreSqlConfig = null)
    {
        string connectionString = BuildPostgresConnectionString(postgreSqlConfig, config);

        services.AddDbContext<ShelflyDbContext>(builder =>
        {
            builder.UseNpgsql(connectionString);
            builder.AddInterceptors(new AuditTimestampInterceptor());
        });

        // TODO: Do we need this?
        services.AddScoped<AuditTimestampInterceptor>();

        return services;
    }
}
