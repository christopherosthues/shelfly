using System.CommandLine;
using Microsoft.Extensions.Logging;
using Shelfly.AdminConsole.Enums;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Services;

namespace Shelfly.AdminConsole.Commands;

public class ViewCommand : Command
{
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
        await ViewKeycloakAsync(outputMode, filePrefix, forceOverwrite);
        await ViewPostgreSqlAsync(outputMode, filePrefix, forceOverwrite);
        await ViewAuthRulesAsync(outputMode, filePrefix, forceOverwrite);
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
        if (config != null)
        {
            if (outputMode == OutputMode.File)
            {
                await ExportToFile(config, BuildExportPath(filePrefix, "keycloak"), forceOverwrite);
            }
            else
            {
                _logger.LogInformation("Keycloak configuration: IssuerUrl={IssuerUrl}, Audience={Audience}",
                    config.IssuerUrl, config.Audience);
            }
        }
        else
        {
            if (outputMode == OutputMode.File)
            {
                await ExportToFile(config, BuildExportPath(filePrefix, "keycloak"), forceOverwrite);
            }
            else
            {
                _logger.LogInformation("Keycloak configuration: not configured");
            }
        }
    }

    private async Task ViewPostgreSqlAsync(OutputMode outputMode, string filePrefix, bool forceOverwrite)
    {
        PostgreSqlConfiguration? config = await _configService.LoadByIdAsync<PostgreSqlConfiguration>(PostgreSqlConfiguration.DefaultId);
        if (config != null)
        {
            if (outputMode == OutputMode.File)
            {
                await ExportToFile(config, BuildExportPath(filePrefix, "postgresql"), forceOverwrite);
            }
            else
            {
                _logger.LogInformation("PostgreSQL configuration: ConnectionString={ConnectionString}",
                    config.ConnectionString);
            }
        }
        else
        {
            if (outputMode == OutputMode.File)
            {
                await ExportToFile(config, BuildExportPath(filePrefix, "postgresql"), forceOverwrite);
            }
            else
            {
                _logger.LogInformation("PostgreSQL configuration: not configured");
            }
        }
    }

    private async Task ViewAuthRulesAsync(OutputMode outputMode, string filePrefix, bool forceOverwrite)
    {
        AuthorizationRule? authRules = await _configService.LoadByIdAsync<AuthorizationRule>(AuthorizationRule.DefaultId);
        if (authRules != null)
        {
            if (outputMode == OutputMode.File)
            {
                await ExportToFile(authRules, BuildExportPath(filePrefix, "auth-rules"), forceOverwrite);
            }
            else
            {
                foreach (Rule rule in authRules.Rules)
                {
                    _logger.LogInformation("Rule: Endpoint={EndpointPattern}, Roles=[{Roles}]",
                        rule.EndpointPattern, string.Join(", ", rule.RequiredRoles));
                }
            }
        }
        else
        {
            if (outputMode == OutputMode.File)
            {
                await ExportToFile(authRules, BuildExportPath(filePrefix, "auth-rules"), forceOverwrite);
            }
            else
            {
                _logger.LogInformation("Authorization rules: not configured");
            }
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
            ? System.Text.Json.JsonSerializer.Serialize(config)
            : "{}";

        await File.WriteAllTextAsync(filePath, jsonContent);
        _logger.LogInformation("Configuration exported to {FilePath}", filePath);
    }

    private static string BuildExportPath(string filePrefix, string configType) => $"{filePrefix}_{configType}.json";
}
