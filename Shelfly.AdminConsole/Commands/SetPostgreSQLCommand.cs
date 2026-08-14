using System.CommandLine;
using Microsoft.Extensions.Logging;
using FluentValidation.Results;
using Shelfly.Configuration;
using Shelfly.AdminConsole.Services;
using Shelfly.AdminConsole.Validation;

namespace Shelfly.AdminConsole.Commands;

public class SetPostgreSQLCommand : Command
{
    private readonly ILogger<SetPostgreSQLCommand> _logger;
    private readonly ConfigService _configService;
    private readonly Option<FileInfo> _fileOption;

    public SetPostgreSQLCommand(ILogger<SetPostgreSQLCommand> logger, ConfigService configService)
        : base("postgresql", "Set PostgreSQL configuration from JSON file")
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
                using var stream = fileInfo.OpenRead();
            }
            catch (IOException ex)
            {
                result.AddError($"Access denied: '{fileInfo.FullName}': {ex.Message}");
            }
        });

        Options.Add(_fileOption);
        SetAction(SetPostgreSqlConfigAsync);
    }

    private async Task<int> SetPostgreSqlConfigAsync(ParseResult parseResult)
    {
        FileInfo? fileInfo = parseResult.GetValue(_fileOption);

        if (fileInfo == null)
        {
            _logger.LogError("Error: --file option is required");
            return 1;
        }

        string jsonContent = await File.ReadAllTextAsync(fileInfo.FullName);
        PostgreSQLConfiguration? config = System.Text.Json.JsonSerializer.Deserialize<PostgreSQLConfiguration>(jsonContent);

        if (config == null)
        {
            _logger.LogError("Error: Failed to deserialize JSON");
            return 1;
        }

        PostgreSQLConfigValidator validator = new();
        ValidationResult result = await validator.ValidateAsync(config);

        if (!result.IsValid)
        {
            foreach (ValidationFailure error in result.Errors)
            {
                _logger.LogWarning("Validation Error: {PropertyName} - {ErrorMessage}", error.PropertyName, error.ErrorMessage);
            }
            return 1;
        }

        PostgreSQLConfiguration? existing =
            await _configService.LoadByIdAsync<PostgreSQLConfiguration>(PostgreSQLConfiguration.DefaultId);
        if (existing == null)
        {
            await _configService.InsertConfigAsync(config);
        }
        else
        {
            await _configService.UpdateConfigAsync(config);
        }

        _logger.LogInformation("PostgreSQL configuration persisted successfully with _id: {postgresql-config}", PostgreSQLConfiguration.DefaultId);
        return 0;
    }
}
