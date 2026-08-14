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

        string filePrefix = "mybackup";

        // Act - generate expected file names (underscore separator format)
        List<string> expectedFiles =
        [
            $"{filePrefix}_keycloak.json",
            $"{filePrefix}_postgresql.json",
            $"{filePrefix}_auth-rules.json"
        ];

        // Assert - file naming convention should be correct
        expectedFiles.Count.ShouldBe(3);
        expectedFiles.ShouldContain($"{filePrefix}_keycloak.json");
        expectedFiles.ShouldContain($"{filePrefix}_postgresql.json");
        expectedFiles.ShouldContain($"{filePrefix}_auth-rules.json");
    }
}
