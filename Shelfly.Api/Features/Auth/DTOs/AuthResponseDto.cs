namespace Shelfly.Api.Features.Auth.DTOs;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    int RefreshExpiresIn,
    string UserId);
