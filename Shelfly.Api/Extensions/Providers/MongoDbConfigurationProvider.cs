using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
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
    private static readonly ActivitySource ActivitySource = new("shelfly-config-provider");

    private static ILogger? _logger;
    private static Counter<long>? _failedRequestsCounter;
    private static Histogram<double>? _pollDurationHistogram;

    private PeriodicTimer? _pollTimer;
    private Task? _pollTask;
    private readonly CancellationTokenSource _pollCancellation = new();

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Sets the logger factory for structured logging.
    /// </summary>
    public static void SetLoggerFactory(ILoggerFactory? loggerFactory) =>
        _logger = loggerFactory?.CreateLogger<MongoDbConfigurationProvider>();

    /// <summary>
    /// Sets the meter for OTel metrics collection.
    /// </summary>
    public static void SetMeter(Meter? meter)
    {
        _failedRequestsCounter = meter?.CreateCounter<long>(
            "shelfly.config.failed_requests",
            "count",
            "Number of failed MongoDB configuration retrievals");

        _pollDurationHistogram = meter?.CreateHistogram<double>(
            "shelfly.config.poll_duration",
            "ms",
            "Duration of MongoDB configuration poll operations");
    }

    public override void Load()
    {
        long elapsedMs = MeasureDuration(() =>
        {
            using Activity? activity = ActivitySource.StartActivity("Initial config load");
            activity?.SetTag("config.operation", "load");

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
                    activity?.SetTag("config.doc_found", true);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger?.LogInformation("MongoDB configuration loaded successfully");
                }
                else
                {
                    Data = ServerDynamicConfiguration.Default().ToFlatJsonDictionary();
                    activity?.SetTag("config.doc_found", false);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger?.LogWarning("No MongoDB configuration document found, using defaults");
                }
            }
            catch (Exception ex)
            {
                RecordFailedRequest("load");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("config.exception", ex.GetType().Name);
                _logger?.LogError(ex, "MongoDB configuration load failed");
                Data = ServerDynamicConfiguration.Default().ToFlatJsonDictionary();
            }
        });

        RecordPollDuration("load", elapsedMs);

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
        using Activity? activity = ActivitySource.StartActivity("Config poll");
        activity?.SetTag("config.operation", "poll");

        double elapsedMs = await MeasureDurationAsync(async () =>
        {
            try
            {
                using MongoClient client = new(connectionString);
                IMongoCollection<BsonDocument> configCollection = client.GetDatabase("shelfly").GetCollection<BsonDocument>("server_configuration");
                BsonDocument? doc = await configCollection.Find(d => d["_id"] == "global_config").FirstOrDefaultAsync(_pollCancellation.Token);

                if (doc == null)
                {
                    _logger?.LogInformation("MongoDB poll: no configuration document found");
                    activity?.SetTag("config.doc_found", false);
                    return;
                }

                string json = doc.ToJson();
                ServerDynamicConfiguration? dbConfig = JsonSerializer.Deserialize<ServerDynamicConfiguration>(json, _jsonSerializerOptions);

                if (dbConfig == null)
                {
                    _logger?.LogWarning("MongoDB poll: configuration document found but deserialization returned null");
                    activity?.SetTag("config.deserialized", false);
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

                    activity?.SetTag("config.updated", true);
                    _logger?.LogInformation("MongoDB configuration updated from poll");
                }
                else
                {
                    activity?.SetTag("config.updated", false);
                }

                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (OperationCanceledException)
            {
                activity?.SetTag("config.canceled", true);
                _logger?.LogInformation("MongoDB poll canceled during shutdown");
            }
            catch (Exception ex)
            {
                RecordFailedRequest("poll");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("config.exception", ex.GetType().Name);
                _logger?.LogWarning(ex, "MongoDB poll failed, retrying on next interval");
            }
        });

        RecordPollDuration("poll", elapsedMs);
    }

    private static long MeasureDuration(Action action)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        action();
        return stopwatch.ElapsedMilliseconds;
    }

    private static async Task<double> MeasureDurationAsync(Func<Task> action)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await action();
        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static void RecordFailedRequest(string operation) =>
        _failedRequestsCounter?.Add(1, new TagList { { "operation", operation } });

    private static void RecordPollDuration(string operation, double durationMs) =>
        _pollDurationHistogram?.Record(durationMs, new TagList { { "operation", operation } });

    public void Dispose()
    {
        _pollCancellation.Cancel();
        _pollCancellation.Dispose();
        _pollTimer?.Dispose();
        _pollTask?.Dispose();
    }
}
