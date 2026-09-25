using MongoDB.Driver;
using Shelfly.Api.Extensions.Providers;
using Shelfly.Api.Features.Admin.Services;
using Shelfly.Configuration;

namespace Shelfly.Api.Extensions;

/// <summary>
/// Extension methods for registering dynamic options infrastructure.
/// </summary>
public static class MongoDbOptionsExtensions
{
    private const string ConfigurationSectionName = nameof(ServerDynamicConfiguration);

    /// <summary>
    /// Composes a MongoDB connection string from individual configuration parts.
    /// </summary>
    public static string BuildMongoConnectionString(IConfigurationRoot config)
    {
        string host = config.GetValue<string>("MongoDB:Host")
                       ?? throw new InvalidOperationException("MongoDB:Host not configured");

        int port = config.GetValue<int?>("MongoDB:Port")
                    ?? 27017;

        string username = config.GetValue<string>("MongoDB:Username")
                          ?? throw new InvalidOperationException("MongoDB:Username not configured");

        string password = config.GetValue<string>("MongoDB:Password")
                            ?? ReadSecretFile(config.GetValue<string?>("MONGODB_PASSWORD_FILE"))
                            ?? throw new InvalidOperationException("MongoDB:Password not configured");

        string database = config.GetValue<string>("MongoDB:Database")
                          ?? "shelfly";

        string authSource = config.GetValue<string>("MongoDB:AuthSource")
                              ?? throw new InvalidOperationException("MongoDB:AuthSource not configured");

        return $"mongodb://{username}:{password}@{host}:{port}/{database}?authSource={authSource}";
    }

    /// <summary>
    /// Reads a secret value from a file path (Docker Compose secrets mount point).
    /// </summary>
    private static string? ReadSecretFile(string? filePath) =>
        filePath is not null && File.Exists(filePath) ? File.ReadAllText(filePath).Trim() : default;

    public static IConfigurationManager AddMongoDbConfiguration(this IConfigurationManager configurationManager, string mongoConnectionString)
    {
        configurationManager.Add(new MongoDbConfigurationSource(mongoConnectionString));

        return configurationManager;
    }

    /// <summary>
    /// Registers the dynamic configuration system backed by MongoDB.
    /// Other services can inject <c>IOptionsMonitor{ServerDynamicConfiguration}</c> to receive reactive updates.
    /// </summary>
    public static IServiceCollection AddMongoDbOptions(this IServiceCollection services, IConfigurationManager configurationManager, string mongoConnectionString)
    {
        // Register infrastructure services
        services.AddSingleton<IMongoDatabase>(_ =>
        {
            MongoClient client = new MongoClient(mongoConnectionString);
            return client.GetDatabase("shelfly");
        });

        services.AddSingleton<DynamicOptionsManager>();

        services.AddOptionsWithValidateOnStart<ServerDynamicConfiguration>()
            .Bind(configurationManager.GetSection(ConfigurationSectionName))
            .ValidateDataAnnotations()
            .Validate(options =>
            {
                // Cross-property validation: when maintenance mode is enabled,
                // ensure telemetry sampling is reasonable to reduce overhead
                if (options.Features.MaintenanceModeEnabled && options.Diagnostics.TelemetrySamplingRate > 0.5)
                {
                    return false;
                }

                return true;
            }, $"{nameof(DiagnosticsConfig.TelemetrySamplingRate)} should be <= 0.5 when {nameof(FeatureToggles.MaintenanceModeEnabled)}.");

        return services;
    }
}
