using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Shelfly.App.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private const int RefreshThresholdMinutes = 5;

    public AuthTokenHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? accessToken = await SecureTokenStore.GetAccessTokenAsync();

        if (accessToken is not null && IsTokenExpiredOrNearExpiry(accessToken))
        {
            await RefreshTokensAsync(accessToken);
        }

        string? updatedAccessToken = await SecureTokenStore.GetAccessTokenAsync();
        if (updatedAccessToken is not null)
        {
            request.Headers.Add("Authorization", $"Bearer {updatedAccessToken}");
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && updatedAccessToken is not null)
        {
            string? refreshToken = await SecureTokenStore.GetRefreshToken();
            if (refreshToken is not null)
            {
                await RefreshTokensAsync(updatedAccessToken);
                HttpResponseMessage newResponse = await base.SendAsync(request, cancellationToken);
                return newResponse;
            }
        }

        return response;
    }

    private bool IsTokenExpiredOrNearExpiry(string token)
    {
        try
        {
            string[] parts = token.Split('.');
            if (parts.Length != 3)
            {
                return true;
            }

            byte[] payload = Convert.FromBase64String(AddPadding(parts[1]));
            Dictionary<string, object>? claims = JsonSerializer.Deserialize<Dictionary<string, object>>(payload);

            if (claims?.TryGetValue("exp", out object? expObj) == true && expObj is double exp)
            {
                DateTime expiryTime = DateTimeOffset.FromUnixTimeSeconds((long)exp).DateTime;
                TimeSpan timeToExpiry = expiryTime - DateTime.UtcNow;
                return timeToExpiry.TotalMinutes < RefreshThresholdMinutes;
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

    private async Task RefreshTokensAsync(string accessToken)
    {
        try
        {
            HttpClient httpClient = new();
            httpClient.BaseAddress = new("http://localhost:5000/");

            using HttpRequestMessage request = new(HttpMethod.Post, "http://localhost:5000/auth/refresh");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                string? newAccessToken = body?.GetValueOrDefault("accessToken");
                string? refreshToken = body?.GetValueOrDefault("refreshToken");

                if (newAccessToken is not null)
                {
                    await SecureTokenStore.StoreTokensAsync(newAccessToken, refreshToken);
                }
            }
        }
        catch
        {
            // Fallback: prompt user to re-login manually on next interaction
        }
    }

    private static string AddPadding(string base64)
    {
        int padding = (4 - base64.Length % 4) % 4;
        return base64.PadRight(base64.Length + padding, '=');
    }
}