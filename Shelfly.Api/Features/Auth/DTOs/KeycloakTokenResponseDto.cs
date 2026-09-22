using System.Text.Json.Serialization;

namespace Shelfly.Api.Features.Auth.DTOs;

public class KeycloakTokenResponseDto
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("scope")]
    public required string Scope { get; init; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; init; }

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshTokenExpiresIn { get; init; }

    public static KeycloakTokenResponseDto None = new()
    {
        AccessToken = string.Empty,
        RefreshToken = string.Empty,
        Scope = string.Empty,
        TokenType = "Bearer"
    };
}
