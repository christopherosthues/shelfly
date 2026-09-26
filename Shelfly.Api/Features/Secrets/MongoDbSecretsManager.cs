using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Shelfly.Api.Constants;

namespace Shelfly.Api.Features.Secrets;

/// <summary>
/// Retrieves secrets from MongoDB using a dedicated "secrets" collection.
/// The document stores key/value pairs where keys are defined in the general config (SecretsConfig).
/// </summary>
public class MongoDbSecretsManager(IMongoDatabase mongoDatabase, ILogger<MongoDbSecretsManager> logger) : ISecretsManager
{
    private readonly IMongoCollection<BsonDocument> _secretsCollection =
        mongoDatabase.GetCollection<BsonDocument>(MongoDbConstants.SecretsCollection);

    public async Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            BsonDocument? doc = await _secretsCollection
                .Find(d => d["_id"] == MongoDbConstants.SecretsDocumentId)
                .FirstOrDefaultAsync(cancellationToken);

            if (doc == null)
            {
                logger.LogInformation("Secrets document not found in MongoDB");
                return null;
            }

            string json = doc.ToJson();
            SecretsData? secretsData = JsonSerializer.Deserialize<SecretsData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return secretsData?.Values.GetValueOrDefault(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve secret '{Key}' from MongoDB", key);
            return null;
        }
    }

    public async Task<Dictionary<string, string>> GetAllSecretsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            BsonDocument? doc = await _secretsCollection
                .Find(d => d["_id"] == MongoDbConstants.SecretsDocumentId)
                .FirstOrDefaultAsync(cancellationToken);

            if (doc == null)
            {
                logger.LogInformation("Secrets document not found in MongoDB");
                return new(StringComparer.OrdinalIgnoreCase);
            }

            string json = doc.ToJson();
            SecretsData? secretsData = JsonSerializer.Deserialize<SecretsData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return secretsData?.Values ?? new(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve all secrets from MongoDB");
            return new(StringComparer.OrdinalIgnoreCase);
        }
    }

    public async Task SetSecretAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        try
        {
            BsonDocument? existingDoc = await _secretsCollection
                .Find(d => d["_id"] == MongoDbConstants.SecretsDocumentId)
                .FirstOrDefaultAsync(cancellationToken);

            SecretsData secretsData;

            if (existingDoc != null)
            {
                string json = existingDoc.ToJson();
                secretsData = JsonSerializer.Deserialize<SecretsData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new() { Values = new(StringComparer.OrdinalIgnoreCase) };
            }
            else
            {
                secretsData = new() { Values = new(StringComparer.OrdinalIgnoreCase) };
            }

            secretsData.Values[key] = value;

            await SaveSecretsDataAsync(secretsData, cancellationToken);
            logger.LogInformation("Set secret '{Key}' in MongoDB", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set secret '{Key}' in MongoDB", key);
        }
    }

    public async Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            BsonDocument? existingDoc = await _secretsCollection
                .Find(d => d["_id"] == MongoDbConstants.SecretsDocumentId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingDoc != null)
            {
                string json = existingDoc.ToJson();
                SecretsData? secretsData = JsonSerializer.Deserialize<SecretsData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                secretsData?.Values.Remove(key);

                if (secretsData != null)
                {
                    await SaveSecretsDataAsync(secretsData, cancellationToken);
                }
            }

            logger.LogInformation("Deleted secret '{Key}' from MongoDB", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete secret '{Key}' from MongoDB", key);
        }
    }

    private async Task SaveSecretsDataAsync(SecretsData secretsData, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(secretsData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });

        BsonDocument doc = BsonDocument.Parse(json);
        doc["_id"] = MongoDbConstants.SecretsDocumentId;

        await _secretsCollection.ReplaceOneAsync(
            d => d["_id"] == MongoDbConstants.SecretsDocumentId,
            doc,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    /// <summary>
    /// Internal data structure for storing secrets as key/value pairs.
    /// </summary>
    private sealed class SecretsData
    {
        public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
