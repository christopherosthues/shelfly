using System.Net.Http.Headers;

namespace Shelfly.Api.Features.Auth.Services;

public class KeycloakAdminClient(IHttpClientFactory httpClientFactory, ILogger<KeycloakAdminClient> logger)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Keycloak");
    private readonly ILogger<KeycloakAdminClient> _logger = logger;

    public async Task<HttpResponseMessage> CreateUserAsync(string realm, string email, string password, CancellationToken cancellationToken)
    {
        var requestContent = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("email", email),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("enabled", "true"),
        ]);

        var response = await _httpClient.PostAsync(
            $"admin/realms/{realm}/users",
            requestContent,
            cancellationToken);

        _logger.LogInformation("Created user {Email} in realm {Realm}", email, realm);

        return response;
    }

    public async Task<HttpResponseMessage> GetUserByEmailAsync(string realm, string email, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            $"admin/realms/{realm}/users?email={Uri.EscapeDataString(email)}",
            cancellationToken);

        return response;
    }

    public async Task<HttpResponseMessage> AuthenticateAsync(string issuer, string email, string password, CancellationToken cancellationToken)
    {
        var requestContent = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", "shelfly-api"),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("grant_type", "password"),
        ]);

        var response = await _httpClient.PostAsync(
            $"{issuer}/protocol/openid-connect/token",
            requestContent,
            cancellationToken);

        return response;
    }

    public async Task<HttpResponseMessage> RefreshTokenAsync(string issuer, string refreshToken, CancellationToken cancellationToken)
    {
        var requestContent = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", "shelfly-api"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
        ]);

        var response = await _httpClient.PostAsync(
            $"{issuer}/protocol/openid-connect/token",
            requestContent,
            cancellationToken);

        return response;
    }

    public async Task<HttpResponseMessage> ExecutePasswordResetActionAsync(string realm, string userId, CancellationToken cancellationToken)
    {
        var actions = new[] { "UPDATE_PASSWORD" };

        var response = await _httpClient.PostAsync(
            $"admin/realms/{realm}/users/{userId}/execute-actions-email",
            new StringContent($"[{string.Join(",", actions)}]", MediaTypeHeaderValue.Parse("application/json")),
            cancellationToken);

        return response;
    }
}
