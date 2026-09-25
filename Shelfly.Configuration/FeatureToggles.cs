
namespace Shelfly.Configuration;

public class FeatureToggles : IJsonConfiguration
{
    public bool MaintenanceModeEnabled { get; set; } = false;
    public bool RegistrationEnabled { get; set; } = true;
    public bool RemoteSyncEnabled { get; set; } = true;

    public Dictionary<string, string?> ToFlatJsonDictionary(string prefix, Dictionary<string, string?> dictionary)
    {
        dictionary[prefix + $":{nameof(MaintenanceModeEnabled)}"] = MaintenanceModeEnabled.ToString();
        dictionary[prefix + $":{nameof(RegistrationEnabled)}"] = RegistrationEnabled.ToString();
        dictionary[prefix + $":{nameof(RemoteSyncEnabled)}"] = RemoteSyncEnabled.ToString();
        return dictionary;
    }
}
