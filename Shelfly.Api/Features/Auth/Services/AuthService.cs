using System.Net.Http.Headers;
using System.Text.Json;
using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Common;

namespace Shelfly.Api.Features.Auth.Services;

public class AuthService(KeycloakAdminClient keycloakAdmin, IConfiguration configuration, ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        string realm = configuration.GetValue<string>("Keycloak:Realm") ?? "master";

        try
        {
            // Check if user already exists
            HttpResponseMessage existingUserResponse = await keycloakAdmin.GetUserByEmailAsync(realm, request.Email, cancellationToken);

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
            HttpResponseMessage response = await keycloakAdmin.CreateUserAsync(realm, request.Email, request.Password, cancellationToken);

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

                logger.LogInformation("Registered user {Email} with ID {UserId}", request.Email, userId);

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
            logger.LogWarning("Registration failed for {Email}: {Status} - {Content}", request.Email, response.StatusCode, errorContent);

            return Result<AuthResponseDto>.Failure($"Registration failed: {response.ReasonPhrase ?? "Unknown error"}");
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Registration error for {Email}", request.Email);
            return Result<AuthResponseDto>.Failure($"Registration error: {ex.Message}");
        }
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        string issuer = configuration.GetValue<string>("Keycloak:Issuer")
                       ?? configuration.GetValue<string>("Keycloak:BaseUrl") ?? "http://localhost:8080";

        try
        {
            HttpResponseMessage response = await keycloakAdmin.AuthenticateAsync(issuer, request.Email, request.Password, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                Dictionary<string, object>? tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                string accessToken = tokenData?.GetValueOrDefault("access_token")?.ToString() ?? "";
                string refreshToken = tokenData?.GetValueOrDefault("refresh_token")?.ToString() ?? "";
                string tokenType = tokenData?.GetValueOrDefault("token_type")?.ToString() ?? "Bearer";

                logger.LogInformation("User logged in: {Email}", request.Email);

                return Result<AuthResponseDto>.Success(new AuthResponseDto(
                    accessToken,
                    request.Email,
                    tokenType));
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                logger.LogWarning("Login failed for {Email}: Invalid credentials", request.Email);
                return Result<AuthResponseDto>.Failure("Invalid email or password");
            }

            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Login failed for {Email}: {Status} - {Content}", request.Email, response.StatusCode, errorContent);

            return Result<AuthResponseDto>.Failure($"Login failed: {response.ReasonPhrase ?? "Unknown error"}");
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Login error for {Email}", request.Email);
            return Result<AuthResponseDto>.Failure($"Login error: {ex.Message}");
        }
    }

    public async Task<Result<AuthResponseDto>> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken)
    {
        string issuer = configuration.GetValue<string>("Keycloak:Issuer")
                       ?? configuration.GetValue<string>("Keycloak:BaseUrl") ?? "http://localhost:8080";

        try
        {
            HttpResponseMessage response = await keycloakAdmin.RefreshTokenAsync(issuer, request.RefreshToken, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                Dictionary<string, object>? tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                string accessToken = tokenData?.GetValueOrDefault("access_token")?.ToString() ?? "";
                string refreshToken = tokenData?.GetValueOrDefault("refresh_token")?.ToString() ?? "";
                string tokenType = tokenData?.GetValueOrDefault("token_type")?.ToString() ?? "Bearer";

                logger.LogInformation("Token refreshed successfully");

                return Result<AuthResponseDto>.Success(new AuthResponseDto(
                    accessToken,
                    "",
                    tokenType));
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                logger.LogWarning("Token refresh failed: Expired or invalid refresh token");
                return Result<AuthResponseDto>.Failure("Expired or invalid refresh token");
            }

            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Token refresh failed: {Status} - {Content}", response.StatusCode, errorContent);

            return Result<AuthResponseDto>.Failure($"Refresh failed: {response.ReasonPhrase ?? "Unknown error"}");
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Token refresh error");
            return Result<AuthResponseDto>.Failure($"Refresh error: {ex.Message}");
        }
    }

    public async Task<Result<string>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        string realm = configuration.GetValue<string>("Keycloak:Realm") ?? "master";

        try
        {
            // Find user by email
            HttpResponseMessage existingUserResponse = await keycloakAdmin.GetUserByEmailAsync(realm, request.Email, cancellationToken);

            if (existingUserResponse.IsSuccessStatusCode)
            {
                string content = await existingUserResponse.Content.ReadAsStringAsync(cancellationToken);
                List<Dictionary<string, object>>? users = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (users is not null && users.Count > 0)
                {
                    string userId = users[0].GetValueOrDefault("id")?.ToString() ?? "";

                    // Execute password reset action
                    HttpResponseMessage response = await keycloakAdmin.ExecutePasswordResetActionAsync(realm, userId, cancellationToken);

                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        logger.LogInformation("Password reset initiated for {Email}", request.Email);

                        return Result<string>.Success($"Password reset link sent to {request.Email}");
                    }

                    string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("Password reset failed for {Email}: {Status} - {Content}", request.Email, response.StatusCode, errorContent);

                    return Result<string>.Failure($"Password reset failed: {response.ReasonPhrase ?? "Unknown error"}");
                }
            }

            // User not found - return generic success to avoid email enumeration
            logger.LogInformation("Password reset requested for unregistered email: {Email}", request.Email);
            return Result<string>.Success($"Password reset link sent to {request.Email}");
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Password reset error for {Email}", request.Email);
            return Result<string>.Failure($"Password reset error: {ex.Message}");
        }
    }
}
