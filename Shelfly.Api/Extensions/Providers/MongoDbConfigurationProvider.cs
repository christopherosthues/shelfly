using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Shelfly.Configuration;

namespace Shelfly.Api.Extensions.Providers;

/// <summary>
/// Configuration provider that reads key-value pairs from MongoDB.
/// Polls the database periodically to detect external changes.
/// </summary>
public sealed class MongoDbConfigurationProvider(string connectionString)
    : ConfigurationProvider, IDisposable
{
    private PeriodicTimer? _pollTimer;
    private Task? _pollTask;
    private readonly CancellationTokenSource _pollCancellation = new();

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // TODO: logging and metrics (e.g. failed retrievals

    public override void Load()
    {
        try
        {
            using MongoClient client = new(connectionString);
            IMongoCollection<BsonDocument> configCollection =
                client.GetDatabase("shelfly").GetCollection<BsonDocument>("server_configuration");
            BsonDocument? doc = configCollection.Find(d => d["_id"] == "global_config").ToList().FirstOrDefault();

            if (doc != null)
            {
                string json = doc.ToJson();
                ServerDynamicConfiguration config =
                    JsonSerializer.Deserialize<ServerDynamicConfiguration>(json, _jsonSerializerOptions) ??
                    ServerDynamicConfiguration.Default();

                Data = config.ToFlatJsonDictionary();
            }
            else
            {
                Data = ServerDynamicConfiguration.Default().ToFlatJsonDictionary();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Poll check failed: {ex.Message}");
            Data = ServerDynamicConfiguration.Default().ToFlatJsonDictionary();
        }

        StartPolling();
    }

    /// <summary>
    /// Starts the background poller that checks MongoDB for changes every 30 seconds.
    /// </summary>
    private void StartPolling()
    {
        _pollTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        _pollTask = Task.Run(async () =>
        {
            while (await _pollTimer.WaitForNextTickAsync(_pollCancellation.Token))
            {
                await PollForChangesAsync();
            }
        });
    }

    private async Task PollForChangesAsync()
    {
        try
        {
            using MongoClient client = new(connectionString);
            IMongoCollection<BsonDocument> configCollection = client.GetDatabase("shelfly").GetCollection<BsonDocument>("server_configuration");
            BsonDocument? doc = await configCollection.Find(d => d["_id"] == "global_config").FirstOrDefaultAsync(_pollCancellation.Token);

            if (doc == null)
            {
                return;
            }

            string json = doc.ToJson();
            ServerDynamicConfiguration? dbConfig = JsonSerializer.Deserialize<ServerDynamicConfiguration>(json, _jsonSerializerOptions);

            // Keep the current config active when new config could not be parsed
            if (dbConfig == null)
            {
                return;
            }

            Dictionary<string, string?> newData = dbConfig.ToFlatJsonDictionary();

            if (!Data.SequenceEqual(newData))
            {
                Data.Clear();
                foreach (KeyValuePair<string, string?> kvp in newData)
                {
                    Data[kvp.Key] = kvp.Value;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception ex)
        {
            // Transient MongoDB errors — swallow and retry on next poll
            System.Diagnostics.Debug.WriteLine($"Poll check failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _pollCancellation.Cancel();
        _pollCancellation.Dispose();
        _pollTimer?.Dispose();
        _pollTask?.Dispose();
    }
}
