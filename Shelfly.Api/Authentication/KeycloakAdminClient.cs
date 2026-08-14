using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Shelfly.Configuration;
using Shelfly.Api.Configuration;

namespace Shelfly.Api.Authentication;

public class KeycloakAdminClient(
    HttpClient httpClient,
    ConfigurationService configurationService,
    IMemoryCache cache)
{
    private const string AdminTokenCacheKey = "keycloak-admin-token";
    private readonly TimeSpan _tokenTtl = TimeSpan.FromMinutes(10);

    public async Task<string?> GetAdminAccessTokenAsync()
    {
        if (cache.TryGetValue(AdminTokenCacheKey, out object? cached))
        {
            return (string?)cached;
        }

        KeycloakConfiguration? config = await configurationService.LoadKeycloakConfigAsync();
        if (config is null)
        {
            throw new InvalidOperationException("Keycloak configuration not found in MongoDB");
        }

        string issuerUrl = config.IssuerUrl;
        string tokenEndpoint = $"{issuerUrl}/protocol/openid-connect/token";

        FormUrlEncodedContent content = new([
            new("client_id", config.AdminClientId),
            new("client_secret", config.AdminClientSecret),
            new("grant_type", "client_credentials"),
            new("audience", issuerUrl)
        ]);

        using HttpResponseMessage response = await httpClient.PostAsync(tokenEndpoint, content);
        if (response.IsSuccessStatusCode)
        {
            Dictionary<string, object>? body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            string? accessToken = body?.GetValueOrDefault("access_token")?.ToString();

            if (accessToken is not null)
            {
                cache.Set(AdminTokenCacheKey, accessToken, _tokenTtl);
            }

            return accessToken;
        }

        throw new HttpRequestException($"Keycloak admin token request failed with status code {response.StatusCode}");
    }

    public async Task<UserResponse?> CreateUserAsync(string email, string password)
    {
        string? adminToken = await GetAdminAccessTokenAsync();
        KeycloakConfiguration? config = await configurationService.LoadKeycloakConfigAsync();
        if (config is null)
        {
            throw new InvalidOperationException("Keycloak configuration not found in MongoDB");
        }

        string realmUrl = $"{config.IssuerUrl}/admin/realms/shelfly/users";

        Dictionary<string, object> userPayload = new()
        {
            ["email"] = email.ToLowerInvariant(),
            ["username"] = email.ToLowerInvariant(),
            ["enabled"] = true,
            ["emailVerified"] = true,
            ["credentials"] = new Dictionary<string, object> { ["type"] = "password", ["value"] = password }
        };

        using HttpRequestMessage request = new(HttpMethod.Post, realmUrl)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(userPayload),
                System.Text.Encoding.UTF8,
                "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {adminToken}");

        using HttpResponseMessage response = await httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            return new(true, null);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return new(false, "DuplicateEmail");
        }

        throw new HttpRequestException($"Keycloak user creation failed with status code {response.StatusCode}");
    }

    public async Task<UserResponse?> FindUserByEmailAsync(string email)
    {
        string? adminToken = await GetAdminAccessTokenAsync();
        KeycloakConfiguration? config = await configurationService.LoadKeycloakConfigAsync();
        if (config is null)
        {
            throw new InvalidOperationException("Keycloak configuration not found in MongoDB");
        }

        string realmUrl = $"{config.IssuerUrl}/admin/realms/shelfly/users?email={Uri.EscapeDataString(email.ToLowerInvariant())}";

        using HttpRequestMessage request = new(HttpMethod.Get, realmUrl);
        request.Headers.Add("Authorization", $"Bearer {adminToken}");

        using HttpResponseMessage response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            UserDto[]? users = await response.Content.ReadFromJsonAsync<UserDto[]>();
            return users?.Length > 0 ? new(true, null) : new UserResponse(false, "UserNotFound");
        }

        throw new HttpRequestException($"Keycloak user lookup failed with status code {response.StatusCode}");
    }

    public async Task<string?> GetUserIdByEmailAsync(string email)
    {
        string? adminToken = await GetAdminAccessTokenAsync();
        KeycloakConfiguration? config = await configurationService.LoadKeycloakConfigAsync();
        if (config is null)
        {
            throw new InvalidOperationException("Keycloak configuration not found in MongoDB");
        }

        string realmUrl = $"{config.IssuerUrl}/admin/realms/shelfly/users?email={Uri.EscapeDataString(email.ToLowerInvariant())}";

        using HttpRequestMessage request = new(HttpMethod.Get, realmUrl);
        request.Headers.Add("Authorization", $"Bearer {adminToken}");

        using HttpResponseMessage response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            UserDto[]? users = await response.Content.ReadFromJsonAsync<UserDto[]>();
            return users?.FirstOrDefault()?.Id;
        }

        throw new HttpRequestException($"Keycloak user lookup failed with status code {response.StatusCode}");
    }

    public async Task SendPasswordResetEmailAsync(string email)
    {
        string? userId = await GetUserIdByEmailAsync(email);
        if (userId is null)
        {
            throw new KeycloakUserNotFoundException($"No user found with email: {email}");
        }

        string? adminToken = await GetAdminAccessTokenAsync();
        KeycloakConfiguration? config = await configurationService.LoadKeycloakConfigAsync();
        if (config is null)
        {
            throw new InvalidOperationException("Keycloak configuration not found in MongoDB");
        }

        string actionUrl = $"{config.IssuerUrl}/admin/realms/shelfly/users/{userId}/actions/update-password";

        using HttpRequestMessage request = new(HttpMethod.Post, actionUrl);
        request.Headers.Add("Authorization", $"Bearer {adminToken}");

        using HttpResponseMessage response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Keycloak password reset failed with status code {response.StatusCode}");
        }
    }

    public async Task<TokenResponse?> AuthenticateLoginAsync(string email, string password)
    {
        KeycloakConfiguration? config = await configurationService.LoadKeycloakConfigAsync();
        if (config is null)
        {
            throw new InvalidOperationException("Keycloak configuration not found in MongoDB");
        }

        string tokenEndpoint = $"{config.IssuerUrl}/protocol/openid-connect/token";

        FormUrlEncodedContent content = new([
            new("client_id", config.AdminClientId),
            new("client_secret", config.AdminClientSecret),
            new("grant_type", "password"),
            new("username", email.ToLowerInvariant()),
            new("password", password)
        ]);

        using HttpResponseMessage response = await httpClient.PostAsync(tokenEndpoint, content);

        if (response.IsSuccessStatusCode)
        {
            Dictionary<string, object>? body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            string? accessToken = body?.GetValueOrDefault("access_token")?.ToString();
            string? refreshToken = body?.GetValueOrDefault("refresh_token")?.ToString();

            return new(accessToken ?? throw new InvalidOperationException("Access token missing"), refreshToken);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return new(null, "InvalidCredentials");
        }

        throw new HttpRequestException($"Keycloak login failed with status code {response.StatusCode}");
    }

    public async Task LogoutAsync(string accessToken)
    {
        KeycloakConfiguration? config = await configurationService.LoadKeycloakConfigAsync();
        if (config is null)
        {
            throw new InvalidOperationException("Keycloak configuration not found in MongoDB");
        }

        string logoutEndpoint = $"{config.IssuerUrl}/protocol/openid-connect/logout";

        using HttpRequestMessage request = new(HttpMethod.Post, logoutEndpoint);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        using HttpResponseMessage response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NoContent)
        {
            throw new HttpRequestException($"Keycloak logout failed with status code {response.StatusCode}");
        }
    }

    public async Task<TokenResponse?> RefreshTokenAsync(string accessToken)
    {
        KeycloakConfiguration? config = await configurationService.LoadKeycloakConfigAsync();
        if (config is null)
        {
            throw new InvalidOperationException("Keycloak configuration not found in MongoDB");
        }

        string tokenEndpoint = $"{config.IssuerUrl}/protocol/openid-connect/token";

        FormUrlEncodedContent content = new([
            new("client_id", config.AdminClientId),
            new("client_secret", config.AdminClientSecret),
            new("grant_type", "refresh_token"),
            new("access_token", accessToken)
        ]);

        using HttpResponseMessage response = await httpClient.PostAsync(tokenEndpoint, content);

        if (response.IsSuccessStatusCode)
        {
            Dictionary<string, object>? body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            string? newAccessToken = body?.GetValueOrDefault("access_token")?.ToString();
            string? refreshToken = body?.GetValueOrDefault("refresh_token")?.ToString();

            return new(newAccessToken ?? throw new InvalidOperationException("Access token missing"), refreshToken);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return new(null, "InvalidToken");
        }

        throw new HttpRequestException($"Keycloak token refresh failed with status code {response.StatusCode}");
    }
}

public record UserDto(string? Id, string Email);

public record UserResponse(bool Success, string? ErrorCode);

public record TokenResponse(string? AccessToken, string? RefreshToken)
{
    public static TokenResponse Error(string errorCode) => new(null!, errorCode);
}

public class KeycloakUserNotFoundException : Exception
{
    public KeycloakUserNotFoundException(string message) : base(message) { }
}
