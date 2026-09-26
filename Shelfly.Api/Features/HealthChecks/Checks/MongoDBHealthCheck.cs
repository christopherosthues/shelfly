using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates MongoDB configuration store connectivity.
/// </summary>
public class MongoDbHealthCheck(string connectionString) : IHealthCheck
{
    private static readonly ActivitySource Source = new("shelfly-health-checks");

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using Activity? activity = Source.StartActivity($"MongoDB check: {context.Registration.Name}");
        activity?.SetTag("health.check.name", "mongodb");

        using MongoClient client = new MongoClient(connectionString);
        IMongoDatabase database = client.GetDatabase("admin");
        BsonDocument pingCommand = new BsonDocument("ping", 1);

        try
        {
            await database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: cancellationToken);
            HealthCheckResult result = HealthCheckResult.Healthy("MongoDB is reachable");
            activity?.SetTag("health.check.status", "Healthy");
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            HealthCheckResult result = HealthCheckResult.Unhealthy(ex.Message);
            activity?.SetTag("health.check.status", "Unhealthy");
            activity?.SetTag("health.check.exception", ex.GetType().Name);
            activity?.SetStatus(ActivityStatusCode.Error);
            return result;
        }
    }
}
