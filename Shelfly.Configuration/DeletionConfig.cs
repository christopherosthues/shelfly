using System.ComponentModel.DataAnnotations;

namespace Shelfly.Configuration;

public class DeletionConfig : IJsonConfiguration
{
    [Required]
    [Range(1, 365, ErrorMessage = "GracePeriodDays must be between {1} and {2}.")]
    public int GracePeriodDays { get; set; } = 30;

    public Dictionary<string, string?> ToFlatJsonDictionary(string prefix, Dictionary<string, string?> dictionary)
    {
        dictionary[prefix + $":{nameof(GracePeriodDays)}"] = GracePeriodDays.ToString();
        return dictionary;
    }
}
