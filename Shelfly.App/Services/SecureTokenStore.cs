namespace Shelfly.App.Services;

public static class SecureTokenStore
{
    private const string AccessTokenKey = "jwt-access-token";
    private const string RefreshTokenKey = "jwt-refresh-token";

    public static async Task StoreTokensAsync(string accessToken, string? refreshToken)
    {
        await SecureStorage.SetAsync(AccessTokenKey, accessToken);
        if (refreshToken is not null)
        {
            await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
        }
    }

    public static async Task<string?> GetAccessTokenAsync() => await SecureStorage.GetAsync(AccessTokenKey);

    public static async Task<string?> GetRefreshToken() => await SecureStorage.GetAsync(RefreshTokenKey);

    public static void RemoveTokens()
    {
        SecureStorage.Remove(AccessTokenKey);
        SecureStorage.Remove(RefreshTokenKey);
    }
}