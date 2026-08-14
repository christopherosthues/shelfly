namespace Shelfly.Api.Authentication.Models;

public record LoginRequest(
    string Email,
    string Password);