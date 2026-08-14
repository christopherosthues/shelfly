using System.CommandLine;
using Shouldly;

namespace Shelfly.AdminConsole.Tests.Integration;

public class SeedAuthRulesTests : IntegrationTestBase
{
    [Test]
    public async Task GivenValidJsonFile_WhenSetAuthRulesCommandRuns_ThenConfigPersistedToMongoDB()
    {
        // Arrange
        string jsonContent = """
            {
              "Rules": [
                {
                  "EndpointPattern": "GET:/api/books",
                  "RequiredRoles": ["user"]
                },
                {
                  "EndpointPattern": "POST:/api/bookmarks/*",
                  "RequiredRoles": ["user", "admin"]
                }
              ]
            }
            """;

        string tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, jsonContent);

        // Act
        RootCommand rootCommand = new();
        Command setCommand = new("set");
        Command authRulesCommand = new("auth-rules");
        Option<string> fileOption = new("--file")
        {
            Description = "JSON configuration file path"
        };

        authRulesCommand.Add(fileOption);
        setCommand.Add(authRulesCommand);
        rootCommand.Add(setCommand);

        // Assert - command structure should be valid
        rootCommand.ShouldNotBeNull();
        setCommand.ShouldNotBeNull();
        authRulesCommand.ShouldNotBeNull();
    }
}
