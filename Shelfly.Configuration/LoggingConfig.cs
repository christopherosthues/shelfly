using System.ComponentModel.DataAnnotations;

namespace Shelfly.Configuration;

public class LoggingConfig : IJsonConfiguration
{
    [Required]
    [RegularExpression(@"^(Trace|Debug|Information|Warning|Error|Critical)$", ErrorMessage = "LogLevel must be one of: Trace, Debug, Information, Warning, Error, Critical.")]
    public string LogLevel { get; set; } = "Information";

    public Dictionary<string, string?> ToFlatJsonDictionary(string prefix, Dictionary<string, string?> dictionary)
    {
        dictionary[prefix + $":{nameof(LogLevel)}"] = LogLevel;
        return dictionary;
    }
}
