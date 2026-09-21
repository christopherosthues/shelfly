namespace Shelfly.Api.Features.Auth.DTOs;

public record LoginRequestDto(
    string Email,
    string Password);
