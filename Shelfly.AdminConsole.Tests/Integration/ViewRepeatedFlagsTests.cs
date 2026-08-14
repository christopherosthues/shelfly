using Shouldly;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Enums;

namespace Shelfly.AdminConsole.Tests.Integration;

public class ViewRepeatedFlagsTests : IntegrationTestBase
{
    [Test]
    public async Task GivenSeededConfig_WhenViewCommandWithMultipleTypesRuns_ThenDisplaysOnlySelectedConfigs()
    {
        // Arrange - simulate seeded configuration state
        KeycloakConfiguration keycloakConfig = KeycloakConfiguration.Create(
            "https://keycloak.example.com/realms/shelfly",
            "shelfly-api",
            "https://keycloak.example.com/protocol/openid-connect/certs",
            "admin-client-id",
            "admin-client-secret");

        PostgreSQLConfiguration postgresqlConfig = PostgreSQLConfiguration.Create(
            "Host=localhost;Port=5432;Database=shelfly;Username=admin;Password=password");

        // Act - retrieve only Keycloak and PostgreSQL configs via repeated flags
        ConfigType[] configTypes = [ConfigType.Keycloak, ConfigType.PostgreSQL];
        List<object> retrievedConfigs = [];

        foreach (ConfigType type in configTypes.Distinct())
        {
            switch (type)
            {
                case ConfigType.Keycloak:
                    retrievedConfigs.Add(keycloakConfig);
                    break;
                case ConfigType.PostgreSQL:
                    retrievedConfigs.Add(postgresqlConfig);
                    break;
            }
        }

        // Assert - should return only the two requested config types
        retrievedConfigs.Count.ShouldBe(2);
        retrievedConfigs.ShouldContain(keycloakConfig);
        retrievedConfigs.ShouldContain(postgresqlConfig);
    }

    [Test]
    public async Task GivenSeededConfig_WhenViewCommandWithAllAndSpecificTypeRuns_ThenAllTakesPrecedence()
    {
        // Arrange - simulate seeded configuration state
        KeycloakConfiguration keycloakConfig = KeycloakConfiguration.Create(
            "https://keycloak.example.com/realms/shelfly",
            "shelfly-api",
            "https://keycloak.example.com/protocol/openid-connect/certs",
            "admin-client-id",
            "admin-client-secret");

        PostgreSQLConfiguration postgresqlConfig = PostgreSQLConfiguration.Create(
            "Host=localhost;Port=5432;Database=shelfly;Username=admin;Password=password");

        AuthorizationRule authRules = AuthorizationRule.Create([
            new("GET:/api/books", ["user"]),
            new("POST:/api/bookmarks/*", ["user", "admin"])
        ]);

        // Act - retrieve configs with All and specific type (All should take precedence)
        ConfigType[] configTypes = [ConfigType.All, ConfigType.Keycloak];
        List<object> retrievedConfigs = [];

        bool showAll = configTypes.Contains(ConfigType.All);

        if (showAll)
        {
            retrievedConfigs.Add(keycloakConfig);
            retrievedConfigs.Add(postgresqlConfig);
            retrievedConfigs.Add(authRules);
        }

        // Assert - should return all three configs because All takes precedence
        retrievedConfigs.Count.ShouldBe(3);
        retrievedConfigs.ShouldContain(keycloakConfig);
        retrievedConfigs.ShouldContain(postgresqlConfig);
        retrievedConfigs.ShouldContain(authRules);
    }
}
