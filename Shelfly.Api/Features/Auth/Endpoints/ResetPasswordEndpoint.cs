using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Api.Features.Auth.Services;
using Shelfly.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Auth.Endpoints;

public static class ResetPasswordEndpoint
{
    private static readonly ActivitySource ActivitySource = new("shelfly-api");

    public static async Task<IResult> Handle(
        IAuthService authService,
        ResetPasswordRequestDto request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        using Activity? activity = ActivitySource.StartActivity("Auth.ResetPassword");

        Result<string> result = await authService.ResetPasswordAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Password reset initiated for {Email}", request.Email);

            activity?.SetTag("auth.user.email", request.Email);
            activity?.SetTag("auth.operation.type", "reset_password");
            activity?.SetTag("auth.outcome", "success");
            activity?.SetStatus(ActivityStatusCode.Ok);

            return Ok(new { message = result.Value });
        }

        activity?.SetTag("auth.user.email", request.Email);
        activity?.SetTag("auth.operation.type", "reset_password");
        activity?.SetTag("auth.outcome", "failure");
        activity?.SetStatus(ActivityStatusCode.Error, result.Error ?? "Password reset failed");

        return Json(new ProblemDetails
        {
            Status = 404,
            Title = "Not Found",
            Detail = result.Error,
            Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
        }, statusCode: 404);
    }
}
