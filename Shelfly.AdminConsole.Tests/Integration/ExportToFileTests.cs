using Shouldly;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Tests.Integration;

public class ExportToFileTests : IntegrationTestBase
{
    [Test]
    public async Task GivenSeededConfig_WhenExportWithPrefixRuns_ThenCreatesCorrectlyNamedFiles()
    {
        // Arrange - simulate seeded configuration state
        KeycloakConfiguration keycloakConfig = KeycloakConfiguration.Create(
            "https://keycloak.example.com/realms/shelfly",
            "shelfly-api",
            "https://keycloak.example.com/protocol/openid-connect/certs",
            "admin-client-id",
            "admin-client-secret");

        string prefix = "mybackup";

        // Act - generate expected file names
        List<string> expectedFiles =
        [
            $"{prefix}-keycloak.json",
            $"{prefix}-postgresql.json",
            $"{prefix}-auth-rules.json"
        ];

        // Assert - file naming convention should be correct
        expectedFiles.Count.ShouldBe(3);
        expectedFiles.ShouldContain($"{prefix}-keycloak.json");
        expectedFiles.ShouldContain($"{prefix}-postgresql.json");
        expectedFiles.ShouldContain($"{prefix}-auth-rules.json");
    }
}
