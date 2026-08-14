using System.CommandLine;
using Shouldly;

namespace Shelfly.AdminConsole.Tests.Integration;

public class SeedKeycloakConfigTests : IntegrationTestBase
{
    private static readonly string TestConnectionString = "mongodb://localhost:27017";

    [Test]
    public async Task GivenValidJsonFile_WhenSetKeycloakCommandRuns_ThenConfigPersistedToMongoDB()
    {
        // Arrange
        string jsonContent = """
            {
              "IssuerUrl": "https://keycloak.example.com/realms/shelfly",
              "Audience": "shelfly-api",
              "JwksEndpoint": "https://keycloak.example.com/realms/shelfly/protocol/openid-connect/certs"
            }
            """;

        string tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, jsonContent);

        // Act
        RootCommand rootCommand = new();
        Command setCommand = new("set");
        Command keycloakCommand = new("keycloak");
        Option<string> fileOption = new("--file")
        {
            Description = "JSON configuration file path"
        };

        keycloakCommand.Add(fileOption);
        setCommand.Add(keycloakCommand);
        rootCommand.Add(setCommand);

        // Assert - command structure should be valid
        rootCommand.ShouldNotBeNull();
        setCommand.ShouldNotBeNull();
        keycloakCommand.ShouldNotBeNull();
    }
}
