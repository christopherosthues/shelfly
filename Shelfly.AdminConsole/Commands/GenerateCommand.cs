using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shelfly.AdminConsole.Enums;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Commands;

public class GenerateCommand : Command
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly ILogger<GenerateCommand> _logger;
    private readonly Option<ConfigType[]> _configOption;
    private readonly Option<string?> _filePrefixOption;
    private readonly Option<bool> _forceOption;

    public GenerateCommand(ILogger<GenerateCommand> logger)
        : base("generate", "Generate empty configuration template files")
    {
        _logger = logger;
        _configOption = new("--config")
        {
            Required = false,
            Description = "Configuration type to generate: All, Keycloak, PostgreSQL, MongoDb (repeatable)",
        };

        _filePrefixOption = new("--file-prefix")
        {
            Required = true,
            Description = "Filename prefix for generated files",
        };

        _forceOption = new("-f", "--force")
        {
            Required = false,
            Description = "Overwrite existing export files",
        };

        Options.Add(_configOption);
        Options.Add(_filePrefixOption);
        Options.Add(_forceOption);
        SetAction(GenerateConfigurationsAsync);
    }

    private async Task<int> GenerateConfigurationsAsync(ParseResult parseResult)
    {
        ConfigType[]? configTypes = parseResult.GetValue(_configOption);
        string? filePrefix = parseResult.GetValue(_filePrefixOption);
        bool forceOverwrite = parseResult.GetValue(_forceOption);

        // Default to All if no --config option provided
        if (configTypes == null || configTypes.Length == 0)
        {
            configTypes = [ConfigType.All];
        }

        // Validate file prefix
        if (string.IsNullOrWhiteSpace(filePrefix))
        {
            _logger.LogError("Error: --file-prefix option is required when generating files");
            return 1;
        }

        // Check for All precedence
        bool generateAll = configTypes.Contains(ConfigType.All);

        if (generateAll)
        {
            await GenerateAllConfigurations(filePrefix!, forceOverwrite);
        }
        else
        {
            await GenerateSpecificConfigurations(configTypes, filePrefix!, forceOverwrite);
        }

        return 0;
    }

    private async Task GenerateAllConfigurations(string filePrefix, bool forceOverwrite)
    {
        KeycloakConfiguration keycloak = CreateDefaultKeycloak();
        PostgreSqlConfiguration postgresql = CreateDefaultPostgreSql();
        AuthorizationRule authRules = CreateDefaultAuthRules();

        await ExportToFile(keycloak, BuildExportPath(filePrefix, "keycloak"), forceOverwrite);
        await ExportToFile(postgresql, BuildExportPath(filePrefix, "postgresql"), forceOverwrite);
        await ExportToFile(authRules, BuildExportPath(filePrefix, "auth-rules"), forceOverwrite);
    }

    private async Task GenerateSpecificConfigurations(ConfigType[] configTypes, string filePrefix, bool forceOverwrite)
    {
        foreach (ConfigType type in configTypes.Distinct())
        {
            switch (type)
            {
                case ConfigType.Keycloak:
                    await ExportToFile(CreateDefaultKeycloak(), BuildExportPath(filePrefix, "keycloak"), forceOverwrite);
                    break;
                case ConfigType.PostgreSQL:
                    await ExportToFile(CreateDefaultPostgreSql(), BuildExportPath(filePrefix, "postgresql"), forceOverwrite);
                    break;
                case ConfigType.MongoDb:
                    await ExportToFile(CreateDefaultAuthRules(), BuildExportPath(filePrefix, "auth-rules"), forceOverwrite);
                    break;
            }
        }
    }

    private static KeycloakConfiguration CreateDefaultKeycloak() =>
        new(
            KeycloakConfiguration.DefaultId,
            "",
            "",
            "",
            "",
            "");

    private static PostgreSqlConfiguration CreateDefaultPostgreSql() =>
        new(
            PostgreSqlConfiguration.DefaultId,
            "");

    private static AuthorizationRule CreateDefaultAuthRules() =>
        new(
            AuthorizationRule.DefaultId,
            []);

    private async Task ExportToFile<T>(T config, string filePath, bool forceOverwrite) where T : class
    {
        if (File.Exists(filePath) && !forceOverwrite)
        {
            _logger.LogWarning("Export file '{FilePath}' already exists. Use -f/--force to overwrite.", filePath);
            return;
        }

        string jsonContent = JsonSerializer.Serialize(config, JsonOptions);

        await File.WriteAllTextAsync(filePath, jsonContent);
        _logger.LogInformation("Generated configuration template for {ConfigType} at {FilePath}", 
            Path.GetFileNameWithoutExtension(filePath).Split('_').LastOrDefault(), filePath);
    }

    private static string BuildExportPath(string filePrefix, string configType) => $"{filePrefix}_{configType}.json";
}
