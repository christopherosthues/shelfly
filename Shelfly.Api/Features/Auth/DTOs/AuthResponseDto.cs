namespace Shelfly.Api.Features.Auth.DTOs;

public record AuthResponseDto(
    string UserId,
    string Email,
    string Status);
