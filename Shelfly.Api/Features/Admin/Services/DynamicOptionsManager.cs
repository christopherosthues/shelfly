using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Shelfly.Configuration;

namespace Shelfly.Api.Features.Admin.Services;

/// <summary>
/// Persists dynamic configuration to MongoDB.
/// </summary>
public class DynamicOptionsManager(IMongoDatabase mongoDatabase, ILogger<DynamicOptionsManager> logger)
{
    private readonly IMongoCollection<BsonDocument> _configCollection = mongoDatabase.GetCollection<BsonDocument>("server_configuration");

    /// <summary>
    /// Loads configuration from MongoDB and seeds defaults if empty.
    /// </summary>
    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Loading dynamic configuration from MongoDB");

        BsonDocument? doc = await _configCollection.Find(d => d["_id"] == "global_config").FirstOrDefaultAsync(cancellationToken);

        if (doc != null)
        {
            try
            {
                string json = doc.ToJson();
                ServerDynamicConfiguration config = JsonSerializer.Deserialize<ServerDynamicConfiguration>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? ServerDynamicConfiguration.Default();

                ApplyLoggingConfiguration(config.Logging);
                logger.LogInformation("Loaded dynamic configuration from MongoDB");
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to parse dynamic configuration from MongoDB, using defaults");
                ApplyLoggingConfiguration(ServerDynamicConfiguration.Default().Logging);
            }
        }
        else
        {
            logger.LogInformation("No dynamic configuration found in MongoDB, seeding defaults");
            await SeedDefaultsAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Saves configuration to MongoDB.
    /// </summary>
    public async Task SaveAsync(ServerDynamicConfiguration newConfiguration, CancellationToken cancellationToken)
    {
        logger.LogInformation("Saving dynamic configuration to MongoDB");

        string json = JsonSerializer.Serialize(newConfiguration, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });

        BsonDocument doc = BsonDocument.Parse(json);
        doc["_id"] = "global_config";

        await _configCollection.ReplaceOneAsync(
            d => d["_id"] == "global_config",
            doc,
            new ReplaceOptions { IsUpsert = true }
        );

        ApplyLoggingConfiguration(newConfiguration.Logging);
    }

    private async Task SeedDefaultsAsync(CancellationToken cancellationToken = default)
    {
        ServerDynamicConfiguration defaultConfig = ServerDynamicConfiguration.Default();
        await SaveAsync(defaultConfig, cancellationToken);
    }

    private void ApplyLoggingConfiguration(LoggingConfig logging)
    {
        try
        {
            LogLevel level = logging.LogLevel switch
            {
                "Trace" => LogLevel.Trace,
                "Debug" => LogLevel.Debug,
                "Information" => LogLevel.Information,
                "Warning" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                "Critical" => LogLevel.Critical,
                _ => LogLevel.Information
            };

            // TODO: update log level of logger if possible

            logger.LogInformation("Applied log level: {Level}", level);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply logging configuration");
        }
    }
}
