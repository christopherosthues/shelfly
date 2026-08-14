using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using Shelfly.Configuration;

namespace Shelfly.Api.Configuration;

public class ConfigurationService(ResilientMongoClient mongoClient, IMemoryCache cache)
{
    private const string CacheKeyPrefix = "config:";
    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);

    public async Task<KeycloakConfiguration?> LoadKeycloakConfigAsync()
    {
        string cacheKey = $"{CacheKeyPrefix}keycloak-config";

        if (cache.TryGetValue(cacheKey, out object? cached))
        {
            return (KeycloakConfiguration?)cached;
        }

        KeycloakConfiguration? config = await mongoClient.LoadConfigAsync(
            Builders<KeycloakConfiguration>.Filter.Eq("_id", KeycloakConfiguration.DefaultId));

        if (config is not null)
        {
            cache.Set(cacheKey, config, _cacheTtl);
        }

        return config;
    }

    public async Task LoadAuthRulesAsync()
    {
        string cacheKey = $"{CacheKeyPrefix}auth-rules";

        if (cache.TryGetValue(cacheKey, out object? cached))
        {
            return;
        }

        AuthorizationRule? rules = await mongoClient.LoadConfigAsync(
            Builders<AuthorizationRule>.Filter.Eq("_id", AuthorizationRule.DefaultId));

        if (rules is not null)
        {
            cache.Set(cacheKey, rules, _cacheTtl);
        }
    }

    public async Task RefreshAsync()
    {
        cache.Remove($"{CacheKeyPrefix}keycloak-config");
        cache.Remove($"{CacheKeyPrefix}auth-rules");

        await LoadKeycloakConfigAsync();
        await LoadAuthRulesAsync();
    }

    public async Task SeedDefaultsAsync()
    {
        KeycloakConfiguration defaultKeycloakConfiguration = KeycloakConfiguration.Create(
            "http://keycloak:8080/realms/shelfly",
            "shelfly-api",
            "http://keycloak:8080/realms/shelfly/protocol/openid-connect/certs",
            "shelfly-admin-service",
            "AdminSecret123!");

        AuthorizationRule defaultAuthRules = AuthorizationRule.Create([
            new("/api/books", ["admin", "reader"]),
            new("/api/bookmarks", ["owner"])
        ]);

        await mongoClient.SeedConfigIfEmptyAsync(defaultKeycloakConfiguration);
        await mongoClient.SeedConfigIfEmptyAsync(defaultAuthRules);
    }
}
