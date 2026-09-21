using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Api.Features.Auth.Services;
using Shelfly.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Auth.Endpoints;

public static class RegisterEndpoint
{
    private static readonly ActivitySource ActivitySource = new("shelfly-api");

    public static async Task<IResult> Handle(
        IAuthService authService,
        RegisterRequestDto request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        using Activity? activity = ActivitySource.StartActivity("Auth.Register");

        Result<AuthResponseDto> result = await authService.RegisterAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("User registered: {Email} with ID {UserId}", request.Email, result.Value.UserId);

            activity?.SetTag("auth.user.id", result.Value.UserId);
            activity?.SetTag("auth.user.email", request.Email);
            activity?.SetTag("auth.operation.type", "register");
            activity?.SetTag("auth.outcome", "success");
            activity?.SetStatus(ActivityStatusCode.Ok);

            return Created("", result.Value);
        }

        activity?.SetTag("auth.user.email", request.Email);
        activity?.SetTag("auth.operation.type", "register");
        activity?.SetTag("auth.outcome", "failure");
        activity?.SetStatus(ActivityStatusCode.Error, result.Error ?? "Registration failed");

        return Conflict(new ProblemDetails
        {
            Status = 409,
            Title = "Conflict",
            Detail = result.Error,
            Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
        });
    }
}
