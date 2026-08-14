namespace Shelfly.Configuration;

public record PostgreSqlConfiguration(
    string Id,
    string ConnectionString)
{
    public const string DefaultId = "postgresql";

    public static PostgreSqlConfiguration Create(string connectionString) =>
        new(DefaultId, connectionString);
}
