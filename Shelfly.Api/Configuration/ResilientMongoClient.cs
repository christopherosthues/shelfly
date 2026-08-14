using MongoDB.Driver;
using Polly;

namespace Shelfly.Api.Configuration;

public class ResilientMongoClient(ILogger<ResilientMongoClient> logger)
{
    private IMongoDatabase? _database;

    public void Initialize(string connectionString, string databaseName)
    {
        MongoClient mongoClient = new(connectionString);
        _database = mongoClient.GetDatabase(databaseName);

        logger.LogInformation("ResilientMongoClient initialized with database '{DatabaseName}'", databaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName) =>
        _database!.GetCollection<T>(collectionName);

    public async Task<T?> LoadConfigAsync<T>(FilterDefinition<T> filter)
        where T : class
    {
        IMongoCollection<T> collection = GetCollection<T>("configuration");

        return await Policy.Handle<MongoException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .ExecuteAsync(async () =>
                await collection.Find(filter).SingleOrDefaultAsync());
    }

    public async Task UpdateConfigAsync<T>(T document)
        where T : class
    {
        IMongoCollection<T> collection = GetCollection<T>("configuration");
        string docId = ((dynamic)document).Id.ToString();

        await Policy.Handle<MongoException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .ExecuteAsync(async () =>
                await collection.ReplaceOneAsync(
                    Builders<T>.Filter.Eq("_id", docId),
                    document));
    }

    public async Task SeedConfigIfEmptyAsync<T>(T defaultDocument)
        where T : class
    {
        IMongoCollection<T> collection = GetCollection<T>("configuration");
        string docId = ((dynamic)defaultDocument).Id.ToString();

        T? existing = await Policy.Handle<MongoException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .ExecuteAsync(async () =>
                await collection.FindAsync(Builders<T>.Filter.Eq("_id", docId))
                    .Result.SingleOrDefaultAsync());

        if (existing is null)
        {
            await Policy.Handle<MongoException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .ExecuteAsync(async () =>
                    await collection.InsertOneAsync(defaultDocument));

            logger.LogInformation("Seeded default configuration document '{DocumentType}'", typeof(T).Name);
        }
    }
}
