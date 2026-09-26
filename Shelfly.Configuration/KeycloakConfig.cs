using System.ComponentModel.DataAnnotations;

namespace Shelfly.Configuration;

public class KeycloakConfig : IJsonConfiguration
{
    [Required]
    public string BaseUrl { get; set; } = "http://localhost:8080";

    [Required]
    public string Realm { get; set; } = "master";

    public string? Issuer { get; set; }

    /// <summary>
    /// Key name used to retrieve the Keycloak admin password from the secrets store.
    /// </summary>
    [Required]
    public string AdminPasswordSecretKey { get; set; } = "keycloak_admin_password";

    public Dictionary<string, string?> ToFlatJsonDictionary(string prefix, Dictionary<string, string?> dictionary)
    {
        dictionary[prefix + $":{nameof(BaseUrl)}"] = BaseUrl;
        dictionary[prefix + $":{nameof(Realm)}"] = Realm;
        dictionary[prefix + $":{nameof(Issuer)}"] = Issuer;
        dictionary[prefix + $":{nameof(AdminPasswordSecretKey)}"] = AdminPasswordSecretKey;

        return dictionary;
    }
}
