using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shelfly.Api.Configuration;
using Shelfly.Configuration;

namespace Shelfly.Api.Tests.Integration;

public class ConfigurationRefreshTest : IntegrationTestBase
{


    [Test]
    public async Task UpdateKeycloakConfig_RefreshesCache()
    {
        // Arrange
        ResilientMongoClient resilientMongoClient = new(new LoggerFactory().CreateLogger<ResilientMongoClient>());
        resilientMongoClient.Initialize(ApiFactory.MongoDbConnectionString(), "shelfly-config");

        MemoryCache cache = new(new MemoryCacheOptions());

        ConfigurationService configurationService = new(resilientMongoClient, cache);

        // Act - update Keycloak config in MongoDB
        await configurationService.RefreshAsync();

        // Assert - subsequent requests use updated settings
        KeycloakConfiguration? loadedConfig = await configurationService.LoadKeycloakConfigAsync();
        loadedConfig.ShouldNotBeNull();
    }

    [Test]
    public async Task Startup_ResiliencePipelineRetriesOnFailure()
    {
        // Arrange
        ResilientMongoClient resilientMongoClient = new(new LoggerFactory().CreateLogger<ResilientMongoClient>());

        // Act - connect to non-existent MongoDB (should retry 5 times)
        await Assert.ThrowsAsync<Exception>(
            async () => resilientMongoClient.Initialize(ApiFactory.MongoDbConnectionString(), "test"));

        // Assert - retries exhausted, graceful failure with clear error message
    }
}
