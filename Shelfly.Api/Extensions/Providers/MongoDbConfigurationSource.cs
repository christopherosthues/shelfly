namespace Shelfly.Api.Extensions.Providers;

/// <summary>
/// Configuration source that builds a MongoDB-backed configuration provider.
/// </summary>
public sealed class MongoDbConfigurationSource(string connectionString) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new MongoDbConfigurationProvider(connectionString);
}
