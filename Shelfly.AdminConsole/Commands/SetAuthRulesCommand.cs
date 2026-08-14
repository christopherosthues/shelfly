using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using FluentValidation.Results;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Services;
using Shelfly.AdminConsole.Validation;

namespace Shelfly.AdminConsole.Commands;

public class SetAuthRulesCommand : Command
{
    private readonly ILogger<SetAuthRulesCommand> _logger;
    private readonly ConfigService _configService;
    private readonly Option<FileInfo> _fileOption;

    public SetAuthRulesCommand(ILogger<SetAuthRulesCommand> logger, ConfigService configService)
        : base("auth-rules", "Set authorization rules from JSON file")
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
        SetAction(SetAuthRulesConfigAsync);
    }

    private async Task<int> SetAuthRulesConfigAsync(ParseResult parseResult)
    {
        FileInfo? fileInfo = parseResult.GetValue(_fileOption);

        if (fileInfo == null)
        {
            _logger.LogError("Error: --file option is required");
            return 1;
        }

        string jsonContent = await File.ReadAllTextAsync(fileInfo.FullName);
        AuthorizationRule? config = JsonSerializer.Deserialize<AuthorizationRule>(jsonContent);

        if (config == null)
        {
            _logger.LogError("Error: Failed to deserialize JSON");
            return 1;
        }

        AuthRulesValidator validator = new();
        ValidationResult result = await validator.ValidateAsync(config);

        if (!result.IsValid)
        {
            foreach (ValidationFailure error in result.Errors)
            {
                _logger.LogWarning("Validation Error: {PropertyName} - {ErrorMessage}", error.PropertyName, error.ErrorMessage);
            }
            return 1;
        }

        AuthorizationRule? existing = await _configService.LoadByIdAsync<AuthorizationRule>(AuthorizationRule.DefaultId);
        if (existing == null)
        {
            await _configService.InsertConfigAsync(config);
        }
        else
        {
            await _configService.UpdateConfigAsync(config);
        }

        _logger.LogInformation("Authorization rules persisted successfully with _id: {auth-rules}", AuthorizationRule.DefaultId);
        return 0;
    }
}
