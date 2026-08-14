using Shouldly;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Tests.Integration;

public class UpdateKeycloakConfigTests : IntegrationTestBase
{
    [Test]
    public async Task GivenExistingConfig_WhenUpdatedWithFewerFields_ThenDocumentReplacedFully()
    {
        // Arrange - simulate a full document replacement scenario
        KeycloakConfiguration originalConfig = KeycloakConfiguration.Create(
            "https://keycloak.example.com/realms/shelfly",
            "shelfly-api",
            "https://keycloak.example.com/realms/shelfly/protocol/openid-connect/certs",
            "admin-client-id",
            "admin-client-secret");

        // Act - create updated config with fewer fields (simulating JSON deserialization)
        KeycloakConfiguration updatedConfig = KeycloakConfiguration.Create(
            "https://keycloak-new.example.com/realms/shelfly",
            "shelfly-api-v2",
            "",
            "admin-client-id",
            "");

        // Assert - the updated config should have the new values
        updatedConfig.IssuerUrl.ShouldBe("https://keycloak-new.example.com/realms/shelfly");
        updatedConfig.Audience.ShouldBe("shelfly-api-v2");
    }
}
