using System.Net.Http.Headers;
using System.Text.Json;
using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Common;

namespace Shelfly.Api.Features.Auth.Services;

public class AuthService(KeycloakAdminClient keycloakAdmin, IConfiguration configuration, ILogger<AuthService> logger)
    : IAuthService
{
    private readonly KeycloakAdminClient _keycloakAdmin = keycloakAdmin;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        string realm = _configuration.GetValue<string>("Keycloak:Realm") ?? "master";

        try
        {
            // Check if user already exists
            HttpResponseMessage existingUserResponse = await _keycloakAdmin.GetUserByEmailAsync(realm, request.Email, cancellationToken);

            if (existingUserResponse.IsSuccessStatusCode)
            {
                string content = await existingUserResponse.Content.ReadAsStringAsync(cancellationToken);
                List<Dictionary<string, object>>? users = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (users is not null && users.Count > 0)
                {
                    return Result<AuthResponseDto>.Failure($"User with email {request.Email} already exists");
                }
            }

            // Create new user
            HttpResponseMessage response = await _keycloakAdmin.CreateUserAsync(realm, request.Email, request.Password, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string location = response.Headers.Location?.ToString() ?? "";

                // Extract user ID from Location header or response body
                string userId = location.Split('/').LastOrDefault() ?? "";

                if (string.IsNullOrEmpty(userId))
                {
                    string content = await response.Content.ReadAsStringAsync(cancellationToken);
                    Dictionary<string, object>? body = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (body?.TryGetValue("id", out object? id) ?? false)
                    {
                        userId = id?.ToString() ?? "";
                    }
                }

                _logger.LogInformation("Registered user {Email} with ID {UserId}", request.Email, userId);

                return Result<AuthResponseDto>.Success(new AuthResponseDto(
                    userId,
                    request.Email,
                    "active"));
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return Result<AuthResponseDto>.Failure($"User with email {request.Email} already exists");
            }

            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Registration failed for {Email}: {Status} - {Content}", request.Email, response.StatusCode, errorContent);

            return Result<AuthResponseDto>.Failure($"Registration failed: {response.ReasonPhrase ?? "Unknown error"}");
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Registration error for {Email}", request.Email);
            return Result<AuthResponseDto>.Failure($"Registration error: {ex.Message}");
        }
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        string issuer = _configuration.GetValue<string>("Keycloak:Issuer") 
                       ?? _configuration.GetValue<string>("Keycloak:BaseUrl") ?? "http://localhost:8080";

        try
        {
            HttpResponseMessage response = await _keycloakAdmin.AuthenticateAsync(issuer, request.Email, request.Password, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                Dictionary<string, object>? tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                string accessToken = tokenData?.GetValueOrDefault("access_token")?.ToString() ?? "";
                string refreshToken = tokenData?.GetValueOrDefault("refresh_token")?.ToString() ?? "";
                string tokenType = tokenData?.GetValueOrDefault("token_type")?.ToString() ?? "Bearer";

                _logger.LogInformation("User logged in: {Email}", request.Email);

                return Result<AuthResponseDto>.Success(new AuthResponseDto(
                    accessToken,
                    request.Email,
                    tokenType));
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Login failed for {Email}: Invalid credentials", request.Email);
                return Result<AuthResponseDto>.Failure("Invalid email or password");
            }

            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Login failed for {Email}: {Status} - {Content}", request.Email, response.StatusCode, errorContent);

            return Result<AuthResponseDto>.Failure($"Login failed: {response.ReasonPhrase ?? "Unknown error"}");
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Login error for {Email}", request.Email);
            return Result<AuthResponseDto>.Failure($"Login error: {ex.Message}");
        }
    }
}
