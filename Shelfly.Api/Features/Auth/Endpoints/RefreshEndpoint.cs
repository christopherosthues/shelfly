using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Api.Features.Auth.Services;
using Shelfly.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Auth.Endpoints;

public static class RefreshEndpoint
{
    private static readonly ActivitySource ActivitySource = new("shelfly-api");

    public static async Task<IResult> Handle(
        IAuthService authService,
        RefreshRequestDto request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        using Activity? activity = ActivitySource.StartActivity("Auth.Refresh");

        Result<AuthResponseDto> result = await authService.RefreshAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Token refreshed successfully");

            // Extract user ID from the new access token claims
            string? userId = result.Value.UserId;

            activity?.SetTag("auth.user.id", userId ?? "unknown");
            activity?.SetTag("auth.operation.type", "refresh");
            activity?.SetTag("auth.outcome", "success");
            activity?.SetStatus(ActivityStatusCode.Ok);

            return Ok(result.Value);
        }

        activity?.SetTag("auth.operation.type", "refresh");
        activity?.SetTag("auth.outcome", "failure");
        activity?.SetStatus(ActivityStatusCode.Error, result.Error ?? "Refresh failed");

        return Json(new ProblemDetails
        {
            Status = 401,
            Title = "Unauthorized",
            Detail = result.Error,
            Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
        }, statusCode: 401);
    }
}
