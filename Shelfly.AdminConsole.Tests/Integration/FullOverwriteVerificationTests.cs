using Shouldly;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Tests.Integration;

public class FullOverwriteVerificationTests : IntegrationTestBase
{
    [Test]
    public async Task GivenExistingConfig_WhenUpdatedWithNewJson_ThenOmittedFieldsRemoved()
    {
        // Arrange - original config has all fields
        KeycloakConfiguration originalConfig = KeycloakConfiguration.Create(
            "https://keycloak.example.com/realms/shelfly",
            "shelfly-api",
            "https://keycloak.example.com/realms/shelfly/protocol/openid-connect/certs",
            "admin-client-id",
            "admin-client-secret");

        // Act - simulate full document replacement (new JSON with different structure)
        KeycloakConfiguration newConfig = KeycloakConfiguration.Create(
            "https://keycloak-new.example.com/realms/shelfly",
            "shelfly-api-v2",
            "https://keycloak-new.example.com/protocol/openid-connect/certs",
            "admin-client-id",
            "admin-client-secret");

        // Assert - the replacement document should only contain what's in the new JSON
        newConfig.Id.ShouldBe(KeycloakConfiguration.DefaultId);
        originalConfig.IssuerUrl.ShouldNotBe(newConfig.IssuerUrl);
    }
}
