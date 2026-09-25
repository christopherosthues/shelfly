using System.ComponentModel.DataAnnotations;

namespace Shelfly.Configuration;

public class RateLimitingConfig : IJsonConfiguration
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least {1}.")]
    public int Capacity { get; set; } = 100;

    [Required]
    [Range(1, 3600, ErrorMessage = "WindowSeconds must be between {1} and {2}.")]
    public int WindowSeconds { get; set; } = 60;

    public Dictionary<string, string?> ToFlatJsonDictionary(string prefix, Dictionary<string, string?> dictionary)
    {
        dictionary[prefix + $":{nameof(Capacity)}"] = Capacity.ToString();
        dictionary[prefix + $":{nameof(WindowSeconds)}"] = WindowSeconds.ToString();
        return dictionary;
    }
}
