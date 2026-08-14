using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Shelfly.AdminConsole.Services;

public class ConfigService(ILogger<ConfigService> logger)
{
    private IMongoDatabase? _database;

    public void Initialize(string connectionString, string databaseName = "shelfly-config")
    {
        MongoClient mongoClient = new(connectionString);
        _database = mongoClient.GetDatabase(databaseName);

        logger.LogInformation("ConfigService initialized with database '{DatabaseName}'", databaseName);
    }

    private IMongoCollection<T> GetCollection<T>(string collectionName = "configuration") =>
        _database!.GetCollection<T>(collectionName);

    public async Task<T?> LoadConfigAsync<T>(FilterDefinition<T> filter)
        where T : class
    {
        IMongoCollection<T> collection = GetCollection<T>();

        return await collection.Find(filter).SingleOrDefaultAsync();
    }

    public async Task UpdateConfigAsync<T>(T document)
        where T : class
    {
        IMongoCollection<T> collection = GetCollection<T>();
        string docId = ((dynamic)document).Id.ToString();

        ReplaceOneResult result = await collection.ReplaceOneAsync(
            Builders<T>.Filter.Eq("_id", docId),
            document);

        logger.LogInformation("Updated configuration document '{DocumentType}' with {MatchedCount} matched, {ModifiedCount} modified",
            typeof(T).Name, result.MatchedCount, result.ModifiedCount);
    }

    public async Task InsertConfigAsync<T>(T document)
        where T : class
    {
        IMongoCollection<T> collection = GetCollection<T>();
        string docId = ((dynamic)document).Id.ToString();

        await collection.InsertOneAsync(document);

        logger.LogInformation("Inserted configuration document '{DocumentType}' with _id '{DocId}'",
            typeof(T).Name, docId);
    }

    public async Task<T?> LoadByIdAsync<T>(string id)
        where T : class
    {
        return await LoadConfigAsync(Builders<T>.Filter.Eq("_id", id));
    }
}
