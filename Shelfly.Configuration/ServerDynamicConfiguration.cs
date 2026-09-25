using Microsoft.Extensions.Options;

namespace Shelfly.Configuration;

/// <summary>
/// Dynamic server configuration stored in MongoDB.
/// Uses a class (not record) to allow the Options infrastructure to mutate properties.
/// </summary>
public class ServerDynamicConfiguration : IJsonConfigurationRoot
{
    [ValidateObjectMembers]
    public DeletionConfig Deletion { get; set; } = new();

    [ValidateObjectMembers]
    public PostgreSqlConfig PostgreSql { get; set; } = new();

    [ValidateObjectMembers]
    public FeatureToggles Features { get; set; } = new();

    [ValidateObjectMembers]
    public RateLimitingConfig RateLimiting { get; set; } = new();

    [ValidateObjectMembers]
    public DiagnosticsConfig Diagnostics { get; set; } = new();

    [ValidateObjectMembers]
    public LoggingConfig Logging { get; set; } = new();

    public static ServerDynamicConfiguration Default() => new();

    public Dictionary<string, string?> ToFlatJsonDictionary()
    {
        string prefix = nameof(ServerDynamicConfiguration);
        Dictionary<string, string?> jsonDictionary = new(StringComparer.OrdinalIgnoreCase);
        Deletion.ToFlatJsonDictionary(prefix + $":{nameof(Deletion)}", jsonDictionary);
        PostgreSql.ToFlatJsonDictionary(prefix + $":{nameof(PostgreSql)}", jsonDictionary);
        Features.ToFlatJsonDictionary(prefix + $":{nameof(Features)}", jsonDictionary);
        RateLimiting.ToFlatJsonDictionary(prefix + $":{nameof(RateLimiting)}", jsonDictionary);
        Diagnostics.ToFlatJsonDictionary(prefix + $":{nameof(Diagnostics)}", jsonDictionary);
        Logging.ToFlatJsonDictionary(prefix + $":{nameof(Logging)}", jsonDictionary);

        return jsonDictionary;
    }
}
