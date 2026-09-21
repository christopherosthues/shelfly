using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Shelfly.Api.Features.HealthChecks.Checks;

/// <summary>
/// Validates MongoDB configuration store connectivity.
/// </summary>
public class MongoDbHealthCheck(string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        MongoClient client = new MongoClient(connectionString);
        IMongoDatabase database = client.GetDatabase("admin");
        BsonDocument pingCommand = new BsonDocument("ping", 1);

        try
        {
            await database.RunCommandAsync<BsonDocument>(pingCommand, cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("MongoDB is reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
