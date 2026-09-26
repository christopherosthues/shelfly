using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;
using Shelfly.Api.Constants;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates MongoDB configuration store connectivity.
/// </summary>
public class MongoDbHealthCheck(string connectionString) : IHealthCheck
{
    private static readonly ActivitySource Source = new(ActivitySources.HealthChecks);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using Activity? activity = Source.StartActivity($"MongoDB check: {context.Registration.Name}");
        activity?.SetTag(TagKeys.HealthCheck.Name, HealthCheckNames.MongoDb);

        using MongoClient client = new MongoClient(connectionString);
        IMongoDatabase database = client.GetDatabase(MongoDbConstants.AdminDatabase);
        BsonDocument pingCommand = new BsonDocument(MongoDbConstants.PingCommand, 1);

        try
        {
            await database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: cancellationToken);
            HealthCheckResult result = HealthCheckResult.Healthy("MongoDB is reachable");
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
