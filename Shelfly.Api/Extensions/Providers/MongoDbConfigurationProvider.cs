using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Shelfly.Api.Constants;
using Shelfly.Configuration;

namespace Shelfly.Api.Extensions.Providers;

/// <summary>
/// Configuration provider that reads key-value pairs from MongoDB.
/// Polls the database periodically to detect external changes.
/// </summary>
public sealed class MongoDbConfigurationProvider(string connectionString)
    : ConfigurationProvider, IDisposable
{
    private static readonly ActivitySource ActivitySource = new(ActivitySources.ConfigProvider);

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
            MetricNames.FailedRequestsCounter,
            MetricNames.CountUnit,
            MetricNames.FailedRequestsDescription);

        _pollDurationHistogram = meter?.CreateHistogram<double>(
            MetricNames.PollDurationHistogram,
            MetricNames.MsUnit,
            MetricNames.PollDurationDescription);
    }

    public override void Load()
    {
        long elapsedMs = MeasureDuration(() =>
        {
            using Activity? activity = ActivitySource.StartActivity("Initial config load");
            activity?.SetTag(TagKeys.Config.Operation, OperationNames.Load);

            try
            {
                using MongoClient client = new(connectionString);
                IMongoCollection<BsonDocument> configCollection =
                    client.GetDatabase(MongoDbConstants.ShelflyDatabase).GetCollection<BsonDocument>(MongoDbConstants.ServerConfigurationCollection);
                BsonDocument? doc = configCollection.Find(d => d["_id"] == MongoDbConstants.GlobalConfigDocumentId).ToList().FirstOrDefault();

                if (doc != null)
                {
                    string json = doc.ToJson();
                    ServerDynamicConfiguration config =
                        JsonSerializer.Deserialize<ServerDynamicConfiguration>(json, _jsonSerializerOptions) ??
                        ServerDynamicConfiguration.Default();

                    Data = config.ToFlatJsonDictionary();
                    activity?.SetTag(TagKeys.Config.DocFound, true);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger?.LogInformation("MongoDB configuration loaded successfully");
                }
                else
                {
                    Data = ServerDynamicConfiguration.Default().ToFlatJsonDictionary();
                    activity?.SetTag(TagKeys.Config.DocFound, false);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger?.LogWarning("No MongoDB configuration document found, using defaults");
                }
            }
            catch (Exception ex)
            {
                RecordFailedRequest(OperationNames.Load);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag(TagKeys.Config.Exception, ex.GetType().Name);
                _logger?.LogError(ex, "MongoDB configuration load failed");
                Data = ServerDynamicConfiguration.Default().ToFlatJsonDictionary();
            }
        });

        RecordPollDuration(OperationNames.Load, elapsedMs);

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
        activity?.SetTag(TagKeys.Config.Operation, OperationNames.Poll);

        double elapsedMs = await MeasureDurationAsync(async () =>
        {
            try
            {
                using MongoClient client = new(connectionString);
                IMongoCollection<BsonDocument> configCollection = client.GetDatabase(MongoDbConstants.ShelflyDatabase).GetCollection<BsonDocument>(MongoDbConstants.ServerConfigurationCollection);
                BsonDocument? doc = await configCollection.Find(d => d["_id"] == MongoDbConstants.GlobalConfigDocumentId).FirstOrDefaultAsync(_pollCancellation.Token);

                if (doc == null)
                {
                    _logger?.LogInformation("MongoDB poll: no configuration document found");
                    activity?.SetTag(TagKeys.Config.DocFound, false);
                    return;
                }

                string json = doc.ToJson();
                ServerDynamicConfiguration? dbConfig = JsonSerializer.Deserialize<ServerDynamicConfiguration>(json, _jsonSerializerOptions);

                if (dbConfig == null)
                {
                    _logger?.LogWarning("MongoDB poll: configuration document found but deserialization returned null");
                    activity?.SetTag(TagKeys.Config.Deserialized, false);
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

                    activity?.SetTag(TagKeys.Config.Updated, true);
                    _logger?.LogInformation("MongoDB configuration updated from poll");
                }
                else
                {
                    activity?.SetTag(TagKeys.Config.Updated, false);
                }

                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (OperationCanceledException)
            {
                activity?.SetTag(TagKeys.Config.Canceled, true);
                _logger?.LogInformation("MongoDB poll canceled during shutdown");
            }
            catch (Exception ex)
            {
                RecordFailedRequest(OperationNames.Poll);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag(TagKeys.Config.Exception, ex.GetType().Name);
                _logger?.LogWarning(ex, "MongoDB poll failed, retrying on next interval");
            }
        });

        RecordPollDuration(OperationNames.Poll, elapsedMs);
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
        _failedRequestsCounter?.Add(1, new TagList { { TagKeys.Operation, operation } });

    private static void RecordPollDuration(string operation, double durationMs) =>
        _pollDurationHistogram?.Record(durationMs, new TagList { { TagKeys.Operation, operation } });

    public void Dispose()
    {
        _pollCancellation.Cancel();
        _pollCancellation.Dispose();
        _pollTimer?.Dispose();
        _pollTask?.Dispose();
    }
}
