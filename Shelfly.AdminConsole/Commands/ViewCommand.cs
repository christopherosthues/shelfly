using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shelfly.AdminConsole.Enums;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Services;

namespace Shelfly.AdminConsole.Commands;

public class ViewCommand : Command
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly ILogger<ViewCommand> _logger;
    private readonly ConfigService _configService;
    private readonly Option<ConfigType[]> _configOption;
    private readonly Option<OutputMode> _outputOption;
    private readonly Option<string?> _filePrefixOption;
    private readonly Option<bool> _forceOption;

    public ViewCommand(ILogger<ViewCommand> logger, ConfigService configService)
        : base("view", "View current configuration")
    {
        _logger = logger;
        _configService = configService;
        _configOption = new("--config")
        {
            Required = false,
            Description = "Configuration type to view: All, Keycloak, PostgreSQL, MongoDb (repeatable)",
        };

        _outputOption = new("--output")
        {
            Required = false,
            Description = "Output mode: Console or File",
        };

        _filePrefixOption = new("--file-prefix")
        {
            Required = false,
            Description = "Filename prefix for exported files (required when --output is File)",
        };

        _forceOption = new("-f", "--force")
        {
            Required = false,
            Description = "Overwrite existing export files",
        };

        Options.Add(_configOption);
        Options.Add(_outputOption);
        Options.Add(_filePrefixOption);
        Options.Add(_forceOption);
        SetAction(ViewConfigurationsAsync);
    }

    private async Task<int> ViewConfigurationsAsync(ParseResult parseResult)
    {
        ConfigType[]? configTypes = parseResult.GetValue(_configOption);
        OutputMode outputMode = parseResult.GetValue(_outputOption);
        string? filePrefix = parseResult.GetValue(_filePrefixOption);
        bool forceOverwrite = parseResult.GetValue(_forceOption);

        // Default to All if no --config option provided
        if (configTypes == null || configTypes.Length == 0)
        {
            configTypes = [ConfigType.All];
        }

        // Validate file prefix when output mode is File
        if (outputMode == OutputMode.File && string.IsNullOrWhiteSpace(filePrefix))
        {
            _logger.LogError("Error: --file-prefix option is required when --output is File");
            return 1;
        }

        // Check for All precedence
        bool showAll = configTypes.Contains(ConfigType.All);

        if (showAll)
        {
            await PrintAllConfigurations(outputMode, filePrefix!, forceOverwrite);
        }
        else
        {
            await PrintConfigurations(configTypes, outputMode, filePrefix!, forceOverwrite);
        }

        return 0;
    }

    private async Task PrintAllConfigurations(OutputMode outputMode, string filePrefix, bool forceOverwrite)
    {
        KeycloakConfiguration? keycloak = await _configService.LoadByIdAsync<KeycloakConfiguration>(KeycloakConfiguration.DefaultId);
        PostgreSqlConfiguration? postgresql = await _configService.LoadByIdAsync<PostgreSqlConfiguration>(PostgreSqlConfiguration.DefaultId);
        AuthorizationRule? authRules = await _configService.LoadByIdAsync<AuthorizationRule>(AuthorizationRule.DefaultId);

        if (outputMode == OutputMode.File)
        {
            // File export: individual files per config type
            await ExportToFile(keycloak, BuildExportPath(filePrefix, "keycloak"), forceOverwrite);
            await ExportToFile(postgresql, BuildExportPath(filePrefix, "postgresql"), forceOverwrite);
            await ExportToFile(authRules, BuildExportPath(filePrefix, "auth-rules"), forceOverwrite);
        }
        else
        {
            // Console output: combined JSON structure
            Dictionary<string, object?> combined = new()
            {
                ["keycloak"] = keycloak,
                ["postgresql"] = postgresql,
                ["authRules"] = authRules
            };

            string json = JsonSerializer.Serialize(combined, JsonOptions);
            _logger.LogInformation("{Json}", json);
        }
    }

    private async Task PrintConfigurations(ConfigType[] configTypes, OutputMode outputMode, string filePrefix, bool forceOverwrite)
    {
        foreach (ConfigType type in configTypes.Distinct())
        {
            switch (type)
            {
                case ConfigType.Keycloak:
                    await ViewKeycloakAsync(outputMode, filePrefix, forceOverwrite);
                    break;
                case ConfigType.PostgreSQL:
                    await ViewPostgreSqlAsync(outputMode, filePrefix, forceOverwrite);
                    break;
                case ConfigType.MongoDb:
                    await ViewAuthRulesAsync(outputMode, filePrefix, forceOverwrite);
                    break;
            }
        }
    }

    private async Task ViewKeycloakAsync(OutputMode outputMode, string filePrefix, bool forceOverwrite)
    {
        KeycloakConfiguration? config = await _configService.LoadByIdAsync<KeycloakConfiguration>(KeycloakConfiguration.DefaultId);

        if (outputMode == OutputMode.File)
        {
            await ExportToFile(config, BuildExportPath(filePrefix, "keycloak"), forceOverwrite);
        }
        else
        {
            string json = JsonSerializer.Serialize(config, JsonOptions);
            _logger.LogInformation("{Json}", json);
        }
    }

    private async Task ViewPostgreSqlAsync(OutputMode outputMode, string filePrefix, bool forceOverwrite)
    {
        PostgreSqlConfiguration? config = await _configService.LoadByIdAsync<PostgreSqlConfiguration>(PostgreSqlConfiguration.DefaultId);

        if (outputMode == OutputMode.File)
        {
            await ExportToFile(config, BuildExportPath(filePrefix, "postgresql"), forceOverwrite);
        }
        else
        {
            string json = JsonSerializer.Serialize(config, JsonOptions);
            _logger.LogInformation("{Json}", json);
        }
    }

    private async Task ViewAuthRulesAsync(OutputMode outputMode, string filePrefix, bool forceOverwrite)
    {
        AuthorizationRule? authRules = await _configService.LoadByIdAsync<AuthorizationRule>(AuthorizationRule.DefaultId);

        if (outputMode == OutputMode.File)
        {
            await ExportToFile(authRules, BuildExportPath(filePrefix, "auth-rules"), forceOverwrite);
        }
        else
        {
            string json = JsonSerializer.Serialize(authRules, JsonOptions);
            _logger.LogInformation("{Json}", json);
        }
    }

    private async Task ExportToFile<T>(T? config, string filePath, bool forceOverwrite) where T : class
    {
        // Check overwrite protection
        if (File.Exists(filePath) && !forceOverwrite)
        {
            _logger.LogWarning("Export file '{FilePath}' already exists. Use -f/--force to overwrite.", filePath);
            return;
        }

        string jsonContent = config != null
            ? JsonSerializer.Serialize(config, JsonOptions)
            : "{}";

        await File.WriteAllTextAsync(filePath, jsonContent);
        _logger.LogInformation("Configuration exported to {FilePath}", filePath);
    }

    private static string BuildExportPath(string filePrefix, string configType) => $"{filePrefix}_{configType}.json";
}
