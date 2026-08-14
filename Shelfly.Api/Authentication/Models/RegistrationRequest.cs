namespace Shelfly.Api.Authentication.Models;

public record RegistrationRequest(
    string Email,
    string Password);