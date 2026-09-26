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

    /// <summary>
    /// Key name used to retrieve the PostgreSQL password from the secrets store.
    /// </summary>
    [Required]
    public string PasswordSecretKey { get; set; } = "postgresql_password";

    public Dictionary<string, string?> ToFlatJsonDictionary(string prefix, Dictionary<string, string?> dictionary)
    {
        dictionary[prefix + $":{nameof(Host)}"] = Host;
        dictionary[prefix + $":{nameof(Port)}"] = Port.ToString();
        dictionary[prefix + $":{nameof(Username)}"] = Username;
        dictionary[prefix + $":{nameof(Database)}"] = Database;
        dictionary[prefix + $":{nameof(PasswordSecretKey)}"] = PasswordSecretKey;

        return dictionary;
    }
}
