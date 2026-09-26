namespace Shelfly.Api.Features.Secrets;

/// <summary>
/// Abstracts secret retrieval from an external secrets provider.
/// Implementations can target HashiCorp Vault, Infisical, Azure Key Vault, MongoDB, etc.
/// </summary>
public interface ISecretsManager
{
    /// <summary>
    /// Retrieves a single secret value by its key name.
    /// </summary>
    Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all secrets currently stored in the provider.
    /// </summary>
    Task<Dictionary<string, string>> GetAllSecretsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores or updates a secret value by key name.
    /// </summary>
    Task SetSecretAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a secret by key name.
    /// </summary>
    Task DeleteSecretAsync(string key, CancellationToken cancellationToken = default);
}
