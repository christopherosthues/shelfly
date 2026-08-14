using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Authentication.Models;

namespace Shelfly.Api.Authentication;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder authGroup = routes.MapGroup("/auth");

        // Registration endpoint (T008)
        authGroup.MapPost("/register", RegisterAsync);

        // Login endpoint (T013)
        authGroup.MapPost("/login", LoginAsync);

        // Logout endpoint (T016)
        authGroup.MapPost("/logout", LogoutAsync);

        // Password reset endpoint (T020)
        authGroup.MapPost("/password-reset", PasswordResetAsync);

        // Token refresh endpoint (T021)
        authGroup.MapPost("/refresh", RefreshAsync);

        return routes;

        async Task<IResult> RegisterAsync(
            [FromBody] RegistrationRequest request,
            IValidator<RegistrationRequest> validator,
            KeycloakAdminClient keycloakAdminClient)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return Results.Problem(
                    title: "Validation Error",
                    detail: validationResult.Errors.First().ErrorMessage,
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                    statusCode: 422);
            }

            try
            {
                UserResponse? response = await keycloakAdminClient.CreateUserAsync(request.Email, request.Password);

                if (response?.Success == true)
                {
                    return Results.Created("", new { message = "Account created successfully" });
                }

                if (response?.ErrorCode == "DuplicateEmail")
                {
                    return Results.Problem(
                        title: "Conflict",
                        detail: "An account with this email already exists",
                        type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.1",
                        statusCode: 409);
                }

                return Results.Problem(
                    title: "Validation Error",
                    detail: "Password must be at least 8 characters",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                    statusCode: 422);
            }
            catch (HttpRequestException) when (keycloakAdminClient is not null)
            {
                return Results.Problem(
                    title: "Service Unavailable",
                    detail: "Authentication service temporarily unavailable. Please retry.",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.12",
                    statusCode: 503);
            }
        }

        async Task<IResult> LoginAsync(
            [FromBody] LoginRequest request,
            IValidator<LoginRequest> validator,
            KeycloakAdminClient keycloakAdminClient)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return Results.Problem(
                    title: "Validation Error",
                    detail: validationResult.Errors.First().ErrorMessage,
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                    statusCode: 422);
            }

            try
            {
                TokenResponse? response = await keycloakAdminClient.AuthenticateLoginAsync(request.Email, request.Password);

                if (response?.AccessToken is not null)
                {
                    return Results.Ok(new { accessToken = response.AccessToken, refreshToken = response.RefreshToken });
                }

                if (response?.RefreshToken == "InvalidCredentials")
                {
                    return Results.Problem(
                        title: "Unauthorized",
                        detail: "Email or password is incorrect",
                        type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                        statusCode: 401);
                }

                return Results.Problem(
                    title: "Unauthorized",
                    detail: "Email or password is incorrect",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                    statusCode: 401);
            }
            catch (HttpRequestException) when (keycloakAdminClient is not null)
            {
                return Results.Problem(
                    title: "Service Unavailable",
                    detail: "Authentication service temporarily unavailable. Please retry.",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.12",
                    statusCode: 503);
            }
        }

        async Task<IResult> LogoutAsync(
            HttpContext httpContext,
            KeycloakAdminClient keycloakAdminClient)
        {
            string? accessToken = httpContext.Request.Headers["Authorization"].ToString()[7..]; // Remove "Bearer " prefix

            if (string.IsNullOrEmpty(accessToken))
            {
                return Results.Problem(
                    title: "Unauthorized",
                    detail: "JWT signature or expiration validation failed",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                    statusCode: 401);
            }

            try
            {
                await keycloakAdminClient.LogoutAsync(accessToken);
                return Results.Ok(new { message = "Session ended successfully" });
            }
            catch (HttpRequestException) when (keycloakAdminClient is not null)
            {
                return Results.Problem(
                    title: "Service Unavailable",
                    detail: "Authentication service temporarily unavailable. Please retry.",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.12",
                    statusCode: 503);
            }
        }

        async Task<IResult> PasswordResetAsync(
            [FromBody] PasswordResetRequest request,
            IValidator<PasswordResetRequest> validator,
            KeycloakAdminClient keycloakAdminClient)
        {
            ValidationResult validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return Results.Problem(
                    title: "Validation Error",
                    detail: validationResult.Errors.First().ErrorMessage,
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                    statusCode: 422);
            }

            try
            {
                await keycloakAdminClient.SendPasswordResetEmailAsync(request.Email);
                return Results.Ok(new { message = "Password reset link sent to your email address" });
            }
            catch (KeycloakUserNotFoundException) when (keycloakAdminClient is not null)
            {
                return Results.Problem(
                    title: "Not Found",
                    detail: "No account found with this email address",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                    statusCode: 404);
            }
            catch (HttpRequestException) when (keycloakAdminClient is not null)
            {
                return Results.Problem(
                    title: "Service Unavailable",
                    detail: "Authentication service temporarily unavailable. Please retry.",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.12",
                    statusCode: 503);
            }
        }

        async Task<IResult> RefreshAsync(
            HttpContext httpContext,
            KeycloakAdminClient keycloakAdminClient)
        {
            string? accessToken = httpContext.Request.Headers["Authorization"].ToString()[7..]; // Remove "Bearer " prefix

            if (string.IsNullOrEmpty(accessToken))
            {
                return Results.Problem(
                    title: "Unauthorized",
                    detail: "JWT signature or expiration validation failed",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                    statusCode: 401);
            }

            try
            {
                TokenResponse? response = await keycloakAdminClient.RefreshTokenAsync(accessToken);

                if (response?.AccessToken is not null)
                {
                    return Results.Ok(new { accessToken = response.AccessToken, refreshToken = response.RefreshToken });
                }

                if (response?.RefreshToken == "InvalidToken")
                {
                    return Results.Problem(
                        title: "Unauthorized",
                        detail: "JWT signature or expiration validation failed",
                        type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                        statusCode: 401);
                }

                return Results.Problem(
                    title: "Unauthorized",
                    detail: "JWT signature or expiration validation failed",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.4",
                    statusCode: 401);
            }
            catch (HttpRequestException) when (keycloakAdminClient is not null)
            {
                return Results.Problem(
                    title: "Service Unavailable",
                    detail: "Authentication service temporarily unavailable. Please retry.",
                    type: "https://datatracker.ietf.org/doc/html/rfc780#section-5.3.12",
                    statusCode: 503);
            }
        }
    }
}
