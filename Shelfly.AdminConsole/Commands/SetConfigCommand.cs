using System.CommandLine;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Shelfly.AdminConsole.Services;

namespace Shelfly.AdminConsole.Commands;

internal abstract class SetConfigCommand<TConfig, TValidator> : Command
    where TConfig : class
    where TValidator : AbstractValidator<TConfig>, new()
{
    private readonly Option<FileInfo> _fileOption = new("--file")
    {
        Required = true,
        Description = "JSON configuration file path"
    };

    private readonly ILogger _logger;
    private readonly ConfigService _configService;
    private readonly string _defaultId;

    protected SetConfigCommand(string name, string description, ILogger logger, ConfigService configService,
        string defaultId) : base(name, description)
    {
        _logger = logger;
        _configService = configService;
        _defaultId = defaultId;

        Initialize();
    }

    private void Initialize()
    {
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
        SetAction(SetConfigAsync);
    }

    private async Task<int> SetConfigAsync(ParseResult parseResult)
    {
        FileInfo? fileInfo = parseResult.GetValue(_fileOption);

        if (fileInfo == null)
        {
            _logger.LogError("Error: --file option is required");
            return 1;
        }

        string jsonContent = await File.ReadAllTextAsync(fileInfo.FullName);
        TConfig? config = JsonSerializer.Deserialize<TConfig>(jsonContent);

        if (config == null)
        {
            _logger.LogError("Error: Failed to deserialize JSON");
            return 1;
        }

        AbstractValidator<TConfig> validator = new TValidator();
        ValidationResult result = await validator.ValidateAsync(config);

        if (!result.IsValid)
        {
            foreach (ValidationFailure error in result.Errors)
            {
                _logger.LogWarning("Validation Error: {PropertyName} - {ErrorMessage}", error.PropertyName, error.ErrorMessage);
            }
            return 1;
        }

        TConfig? existing = await _configService.LoadByIdAsync<TConfig>(_defaultId);
        if (existing == null)
        {
            await _configService.InsertConfigAsync(config);
        }
        else
        {
            await _configService.UpdateConfigAsync(config);
        }

        _logger.LogInformation("Configuration persisted successfully with _id: {defaultId}", _defaultId);
        return 0;
    }
}
