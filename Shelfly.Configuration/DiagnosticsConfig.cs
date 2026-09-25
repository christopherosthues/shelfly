using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Shelfly.Configuration;

public class DiagnosticsConfig : IJsonConfiguration
{
    [Required]
    [Range(0.0, 1.0, ErrorMessage = "TelemetrySamplingRate must be between {1} and {2}.")]
    public double TelemetrySamplingRate { get; set; } = 1.0;

    public bool HealthChecksEnabled { get; set; } = true;

    public Dictionary<string, string?> ToFlatJsonDictionary(string prefix, Dictionary<string, string?> dictionary)
    {
        dictionary[prefix + $":{nameof(TelemetrySamplingRate)}"] = TelemetrySamplingRate.ToString(CultureInfo.InvariantCulture);
        dictionary[prefix + $":{nameof(HealthChecksEnabled)}"] = HealthChecksEnabled.ToString();
        return dictionary;
    }
}
