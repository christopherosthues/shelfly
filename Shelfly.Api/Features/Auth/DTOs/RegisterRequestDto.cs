namespace Shelfly.Api.Features.Auth.DTOs;

public record RegisterRequestDto(
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null);
