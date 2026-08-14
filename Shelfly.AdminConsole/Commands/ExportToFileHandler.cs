using System.CommandLine;
using Microsoft.Extensions.Logging;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Services;

namespace Shelfly.AdminConsole.Commands;

public class ExportToFileHandler : Command
{
    private readonly ILogger<ExportToFileHandler> _logger;
    private readonly ConfigService _configService;
    private readonly Option<string> _prefixOption;

    public ExportToFileHandler(ILogger<ExportToFileHandler> logger, ConfigService configService)
        : base("export", "Export configurations to JSON files")
    {
        _logger = logger;
        _configService = configService;
        _prefixOption = new("--prefix")
        {
            Required = true,
            Description = "Filename prefix for exported files"
        };

        _prefixOption.Validators.Add(result =>
        {
            string? prefix = result.GetValue(_prefixOption);
            if (string.IsNullOrWhiteSpace(prefix))
            {
                result.AddError("Prefix cannot be empty");
                return;
            }

            // Validate that the directory containing the prefix path is writable
            string? directory = Path.GetDirectoryName(prefix);
            if (directory != null)
            {
                try
                {
                    using FileStream testFile = File.Create(Path.Combine(directory, ".write-test"), 1, FileOptions.DeleteOnClose);
                }
                catch (IOException ex)
                {
                    result.AddError($"Directory not writable: '{directory}': {ex.Message}");
                }
            }
        });

        Options.Add(_prefixOption);
        SetAction(ExportConfigurationsToFileAsync);
    }

    private async Task<int> ExportConfigurationsToFileAsync(ParseResult parseResult)
    {
        string? prefix = parseResult.GetValue(_prefixOption);

        if (string.IsNullOrWhiteSpace(prefix))
        {
            _logger.LogError("Error: --prefix option is required");
            return 1;
        }

        // Export Keycloak config
        string keycloakFilePath = $"{prefix}-keycloak.json";
        KeycloakConfiguration? keycloakConfig = await _configService.LoadByIdAsync<KeycloakConfiguration>(KeycloakConfiguration.DefaultId);
        await File.WriteAllTextAsync(keycloakFilePath, keycloakConfig != null
            ? System.Text.Json.JsonSerializer.Serialize(keycloakConfig)
            : "{}");
        _logger.LogInformation("Keycloak configuration exported to {keycloakFilePath}", keycloakFilePath);

        // Export PostgreSQL config
        string postgresqlFilePath = $"{prefix}-postgresql.json";
        PostgreSQLConfiguration? postgresqlConfig = await _configService.LoadByIdAsync<PostgreSQLConfiguration>(PostgreSQLConfiguration.DefaultId);
        await File.WriteAllTextAsync(postgresqlFilePath, postgresqlConfig != null
            ? System.Text.Json.JsonSerializer.Serialize(postgresqlConfig)
            : "{}");
        _logger.LogInformation("PostgreSQL configuration exported to {postgresqlFilePath}", postgresqlFilePath);

        // Export Auth rules
        string authRulesFilePath = $"{prefix}-auth-rules.json";
        AuthorizationRule? authRules = await _configService.LoadByIdAsync<AuthorizationRule>(AuthorizationRule.DefaultId);
        await File.WriteAllTextAsync(authRulesFilePath, authRules != null
            ? System.Text.Json.JsonSerializer.Serialize(authRules)
            : "{}");
        _logger.LogInformation("Authorization rules exported to {authRulesFilePath}", authRulesFilePath);

        return 0;
    }
}
