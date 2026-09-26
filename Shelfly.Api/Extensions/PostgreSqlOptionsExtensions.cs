using Microsoft.EntityFrameworkCore;
using Npgsql;
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
    public static string BuildPostgresConnectionString(PostgreSqlConfig? postgreSqlConfig, IConfiguration config)
    {
        string host = postgreSqlConfig?.Host
                        ?? throw new InvalidOperationException("PostgreSql:Host not configured");

        int port = postgreSqlConfig?.Port ?? 5432;

        string username = postgreSqlConfig?.Username
                          ?? throw new InvalidOperationException("PostgreSql:Username not configured");

        string password = ReadSecretFile(config.GetValue<string?>("POSTGRESQL_PASSWORD_FILE"))
                          ?? throw new InvalidOperationException("PostgreSql:Password not configured");

        string database = postgreSqlConfig?.Database ?? "shelfly";

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = host,
            Port = port,
            Username = username,
            Password = password,
            Database = database,
        };

        return builder.ToString();
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
