using System.Net.Http.Headers;
using Shelfly.Api.Constants;

namespace Shelfly.Api.Features.Auth.Services;

public class KeycloakAdminClient(IHttpClientFactory httpClientFactory, ILogger<KeycloakAdminClient> logger)
{
    public async Task<HttpResponseMessage> CreateUserAsync(string realm, string email, string password, CancellationToken cancellationToken)
    {
        FormUrlEncodedContent requestContent = new(
        [
            new KeyValuePair<string, string>("email", email),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("enabled", "true"),
        ]);

        using HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNames.Keycloak);
        HttpResponseMessage response = await httpClient.PostAsync(
            $"admin/realms/{realm}/users",
            requestContent,
            cancellationToken);

        logger.LogInformation("Created user {Email} in realm {Realm}", email, realm);

        return response;
    }

    public async Task<HttpResponseMessage> GetUserByEmailAsync(string realm, string email, CancellationToken cancellationToken)
    {
        using HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNames.Keycloak);
        HttpResponseMessage response = await httpClient.GetAsync(
            $"admin/realms/{realm}/users?email={Uri.EscapeDataString(email)}",
            cancellationToken);

        return response;
    }

    public async Task<HttpResponseMessage> AuthenticateAsync(string issuer, string email, string password, CancellationToken cancellationToken)
    {
        FormUrlEncodedContent requestContent = new(
        [
            new KeyValuePair<string, string>("client_id", "shelfly-api"),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("grant_type", "password"),
        ]);

        using HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNames.Keycloak);
        HttpResponseMessage response = await httpClient.PostAsync(
            $"{issuer}/protocol/openid-connect/token",
            requestContent,
            cancellationToken);

        return response;
    }

    public async Task<HttpResponseMessage> RefreshTokenAsync(string issuer, string refreshToken, CancellationToken cancellationToken)
    {
        FormUrlEncodedContent requestContent = new(
        [
            new KeyValuePair<string, string>("client_id", "shelfly-api"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
        ]);

        using HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNames.Keycloak);
        HttpResponseMessage response = await httpClient.PostAsync(
            $"{issuer}/protocol/openid-connect/token",
            requestContent,
            cancellationToken);

        return response;
    }

    public async Task<HttpResponseMessage> ExecutePasswordResetActionAsync(string realm, string userId, CancellationToken cancellationToken)
    {
        string[] actions = ["UPDATE_PASSWORD"];

        using HttpClient httpClient = httpClientFactory.CreateClient(HttpClientNames.Keycloak);
        HttpResponseMessage response = await httpClient.PostAsync(
            $"admin/realms/{realm}/users/{userId}/execute-actions-email",
            new StringContent($"[{string.Join(",", actions)}]", MediaTypeHeaderValue.Parse("application/json")),
            cancellationToken);

        return response;
    }
}
