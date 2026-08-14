using System.CommandLine;
using Microsoft.Extensions.Logging;
using FluentValidation.Results;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Services;
using Shelfly.AdminConsole.Validation;

namespace Shelfly.AdminConsole.Commands;

public class SetKeycloakCommand : Command
{
    private readonly ILogger<SetKeycloakCommand> _logger;
    private readonly ConfigService _configService;
    private readonly Option<FileInfo> _fileOption;

    public SetKeycloakCommand(ILogger<SetKeycloakCommand> logger, ConfigService configService)
        : base("keycloak", "Set Keycloak configuration from JSON file")
    {
        _logger = logger;
        _configService = configService;
        _fileOption = new("--file")
        {
            Required = true,
            Description = "JSON configuration file path"
        };

        _fileOption.Validators.Add(result =>
        {
            FileInfo? fileInfo = result.GetValue(_fileOption);
            if (fileInfo == null)
            {
                result.AddError("File not found");
                return;
            }

            if (!fileInfo.Exists)
            {
                result.AddError($"File not found: '{fileInfo.FullName}'");
                return;
            }

            try
            {
                using FileStream stream = fileInfo.OpenRead();
            }
            catch (IOException ex)
            {
                result.AddError($"Access denied: '{fileInfo.FullName}': {ex.Message}");
            }
        });

        Options.Add(_fileOption);
        SetAction(SetKeycloakConfigAsync);
    }

    private async Task<int> SetKeycloakConfigAsync(ParseResult parseResult)
    {
        FileInfo? fileInfo = parseResult.GetValue(_fileOption);

        if (fileInfo == null)
        {
            _logger.LogError("Error: --file option is required");
            return 1;
        }

        string jsonContent = await File.ReadAllTextAsync(fileInfo.FullName);
        KeycloakConfiguration? config = System.Text.Json.JsonSerializer.Deserialize<KeycloakConfiguration>(jsonContent);

        if (config == null)
        {
            _logger.LogError("Error: Failed to deserialize JSON");
            return 1;
        }

        KeycloakConfigValidator validator = new();
        ValidationResult result = await validator.ValidateAsync(config);

        if (!result.IsValid)
        {
            foreach (ValidationFailure error in result.Errors)
            {
                _logger.LogWarning("Validation Error: {PropertyName} - {ErrorMessage}", error.PropertyName, error.ErrorMessage);
            }
            return 1;
        }

        KeycloakConfiguration? existing = await _configService.LoadByIdAsync<KeycloakConfiguration>(KeycloakConfiguration.DefaultId);
        if (existing == null)
        {
            await _configService.InsertConfigAsync(config);
        }
        else
        {
            await _configService.UpdateConfigAsync(config);
        }

        _logger.LogInformation("Keycloak configuration persisted successfully with _id: {keycloak}", KeycloakConfiguration.DefaultId);
        return 0;
    }
}
