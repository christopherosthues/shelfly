namespace Shelfly.Configuration;

public record PostgreSQLConfiguration(
    string Id,
    string ConnectionString)
{
    public const string DefaultId = "postgresql";

    public static PostgreSQLConfiguration Create(string connectionString) =>
        new(DefaultId, connectionString);
}
