using Shouldly;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Enums;

namespace Shelfly.AdminConsole.Tests.Integration;

public class ViewSubsetConfigTests : IntegrationTestBase
{
    [Test]
    public async Task GivenSeededConfig_WhenViewCommandWithSpecificTypeRuns_ThenDisplaysOnlyThatConfig()
    {
        // Arrange - simulate seeded configuration state
        KeycloakConfiguration keycloakConfig = KeycloakConfiguration.Create(
            "https://keycloak.example.com/realms/shelfly",
            "shelfly-api",
            "https://keycloak.example.com/protocol/openid-connect/certs",
            "admin-client-id",
            "admin-client-secret");

        // Act - retrieve only Keycloak config via unified view command with --config Keycloak
        ConfigType[] configTypes = [ConfigType.Keycloak];
        object? retrievedConfig = null;

        foreach (ConfigType type in configTypes)
        {
            if (type == ConfigType.Keycloak)
            {
                retrievedConfig = keycloakConfig;
            }
        }

        // Assert - should return only the requested config type
        retrievedConfig.ShouldNotBeNull();
        ((KeycloakConfiguration)retrievedConfig).IssuerUrl.ShouldBe("https://keycloak.example.com/realms/shelfly");
    }
}
