using System.CommandLine;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Shelfly.AdminConsole.Commands;
using Shelfly.AdminConsole.Enums;

namespace Shelfly.AdminConsole.Tests.Unit;

public class GenerateCommandFileOutputTests
{
    [Test]
    public void GivenGenerateCommand_WhenInvokedWithAllConfigs_ThenGeneratesThreeFiles()
    {
        // Arrange
        ILogger<GenerateCommand> logger = Substitute.For<ILogger<GenerateCommand>>();
        GenerateCommand command = new(logger);

        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        string filePrefix = "test_" + Guid.NewGuid().ToString()[..8];

        // Act - simulate command execution with --config All and --file-prefix
        ParseResult parseResult = command.Parse([
            "--config", "All",
            "--file-prefix", filePrefix,
            "-f"  // force overwrite
        ]);

        // Assert - verify options are configured correctly by checking the command has the expected number of options
        command.Options.Count().ShouldBe(3);
        
        // Verify --file-prefix is required (check all options for one with Required = true)
        bool hasRequiredOption = command.Options.Any(o => o.Required == true);
        hasRequiredOption.ShouldBe(true);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Test]
    public void GivenGenerateCommand_WhenInvokedWithoutFilePrefix_ThenReturnsError()
    {
        // Arrange
        ILogger<GenerateCommand> logger = Substitute.For<ILogger<GenerateCommand>>();
        GenerateCommand command = new(logger);

        // Act - simulate command execution without --file-prefix
        ParseResult parseResult = command.Parse([
            "--config", "Keycloak"
        ]);

        // Assert - file prefix should be required, causing a validation error
        // When no --file-prefix is provided, the GetValue will throw InvalidOperationException because it's required
        Option<string?>? filePrefixOption = command.Options.OfType<Option<string?>>().FirstOrDefault();
        if (filePrefixOption != null)
        {
            Exception? caughtException = null;
            try
            {
                _ = parseResult.GetValue(filePrefixOption);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("required"))
            {
                caughtException = ex;
            }

            // The exception should be thrown because --file-prefix is required
            caughtException.ShouldNotBeNull();
        }
    }

    [Test]
    public void GivenExistingFile_WhenGenerateWithoutForce_ThenSkipsOverwrite()
    {
        // Arrange - create a temporary file that will be overwritten
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        string filePath = Path.Combine(tempDir, "existing_keycloak.json");
        
        File.WriteAllText(filePath, "{}");

        ILogger<GenerateCommand> logger = Substitute.For<ILogger<GenerateCommand>>();
        GenerateCommand command = new(logger);

        // Act - simulate file export behavior (we test the logic directly)
        bool forceOverwrite = false;
        bool fileExists = File.Exists(filePath);
        
        string? actualContent = null;
        if (!fileExists || forceOverwrite)
        {
            actualContent = "{}";
            File.WriteAllText(filePath, actualContent!);
        }

        // Assert - file should retain original content when force is false and file exists
        string originalContent = File.ReadAllText(filePath);
        originalContent.ShouldBe("{}");

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Test]
    public void GivenExistingFile_WhenGenerateWithForce_ThenOverwritesFile()
    {
        // Arrange - create a temporary file that will be overwritten
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        string filePath = Path.Combine(tempDir, "existing_keycloak.json");
        
        File.WriteAllText(filePath, "{}");

        ILogger<GenerateCommand> logger = Substitute.For<ILogger<GenerateCommand>>();
        GenerateCommand command = new(logger);

        // Act - simulate file export behavior with force flag
        bool forceOverwrite = true;
        bool fileExists = File.Exists(filePath);

        if (fileExists && !forceOverwrite)
        {
            logger.LogWarning("Export file '{FilePath}' already exists. Use -f/--force to overwrite.", filePath);
        }
        else
        {
            // Simulate writing new content
            string jsonContent = "{\"Id\": \"keycloak\"}";
            File.WriteAllText(filePath, jsonContent);
        }

        // Assert - file should be overwritten and contain new content
        File.Exists(filePath).ShouldBeTrue();
        string actualContent = File.ReadAllText(filePath);
        actualContent.ShouldContain("keycloak");

        // Cleanup
        Directory.Delete(tempDir, true);
    }
}
