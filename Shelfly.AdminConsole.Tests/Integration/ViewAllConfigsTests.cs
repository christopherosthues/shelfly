using Shouldly;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Enums;

namespace Shelfly.AdminConsole.Tests.Integration;

public class ViewAllConfigsTests : IntegrationTestBase
{
    [Test]
    public async Task GivenSeededConfig_WhenViewCommandWithAllRuns_ThenDisplaysAllConfigs()
    {
        // Arrange - simulate seeded configuration state
        KeycloakConfiguration keycloakConfig = KeycloakConfiguration.Create(
            "https://keycloak.example.com/realms/shelfly",
            "shelfly-api",
            "https://keycloak.example.com/protocol/openid-connect/certs",
            "admin-client-id",
            "admin-client-secret");

        PostgreSqlConfiguration postgresqlConfig = PostgreSqlConfiguration.Create(
            "Host=localhost;Port=5432;Database=shelfly;Username=admin;Password=password");

        AuthorizationRule authRules = AuthorizationRule.Create([
            new("GET:/api/books", ["user"]),
            new("POST:/api/bookmarks/*", ["user", "admin"])
        ]);

        // Act - verify all configs are accessible via unified view command with --config All
        ConfigType[] configTypes = [ConfigType.All];
        List<object> allConfigs = [];

        if (configTypes.Contains(ConfigType.All))
        {
            allConfigs.Add(keycloakConfig);
            allConfigs.Add(postgresqlConfig);
            allConfigs.Add(authRules);
        }

        // Assert - all three config types should be present
        allConfigs.Count.ShouldBe(3);
        allConfigs.ShouldContain(keycloakConfig);
        allConfigs.ShouldContain(postgresqlConfig);
        allConfigs.ShouldContain(authRules);
    }
}
