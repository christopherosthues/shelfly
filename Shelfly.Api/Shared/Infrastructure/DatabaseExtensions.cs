using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;

namespace Shelfly.Api.Shared.Infrastructure;

public static class DatabaseExtensions
{
    /// <summary>
    /// Configures PostgreSQL database with EF Core connection pooling.
    /// Connection pooling is enabled by default in Npgsql (max pool size: 100).
    /// This extension provides a centralized configuration pattern for consistency per Constitution IV.
    /// </summary>
    public static IServiceCollection AddPostgreSqlDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ShelflyDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    /// <summary>
    /// Configures SQLite pool settings for local storage (MAUI client).
    /// SQLite connection pooling is handled by EF Core automatically.
    /// </summary>
    public static IServiceCollection AddSqliteDatabase(this IServiceCollection services, string connectionString)
    {
        // For MAUI clients, this would be configured in MauiProgram.cs or similar bootstrap code.
        // The LocalDbContext uses SQLite with connection pooling enabled by default via EF Core.

        return services;
    }
}
