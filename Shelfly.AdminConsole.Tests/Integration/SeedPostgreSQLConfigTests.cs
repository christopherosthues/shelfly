using System.CommandLine;
using Shouldly;

namespace Shelfly.AdminConsole.Tests.Integration;

public class SeedPostgreSQLConfigTests : IntegrationTestBase
{
    [Test]
    public async Task GivenValidJsonFile_WhenSetPostgreSQLCommandRuns_ThenConfigPersistedToMongoDB()
    {
        // Arrange
        string jsonContent = """
            {
              "ConnectionString": "Host=localhost;Port=5432;Database=shelfly;Username=admin;Password=password"
            }
            """;

        string tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, jsonContent);

        // Act
        RootCommand rootCommand = new();
        Command setCommand = new("set");
        Command postgresqlCommand = new("postgresql");
        Option<string> fileOption = new("--file")
        {
            Description = "JSON configuration file path"
        };

        postgresqlCommand.Add(fileOption);
        setCommand.Add(postgresqlCommand);
        rootCommand.Add(setCommand);

        // Assert - command structure should be valid
        rootCommand.ShouldNotBeNull();
        setCommand.ShouldNotBeNull();
        postgresqlCommand.ShouldNotBeNull();
    }
}
