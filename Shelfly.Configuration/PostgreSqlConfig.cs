using System.ComponentModel.DataAnnotations;

namespace Shelfly.Configuration;

public class PostgreSqlConfig : IJsonConfiguration
{
    [Required]
    public string Host { get; set; } = null!;

    [Range(1, 65535)]
    public int Port { get; set; } = 5432;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Database { get; set; } = "shelfly";

    public Dictionary<string, string?> ToFlatJsonDictionary(string prefix, Dictionary<string, string?> dictionary)
    {
        dictionary[prefix + $":{nameof(Host)}"] = Host;
        dictionary[prefix + $":{nameof(Port)}"] = Port.ToString();
        dictionary[prefix + $":{nameof(Username)}"] = Username;
        dictionary[prefix + $":{nameof(Database)}"] = Database;

        return dictionary;
    }
}
