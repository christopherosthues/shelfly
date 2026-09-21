using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Api.Features.Auth.Services;
using Shelfly.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Auth.Endpoints;

public static class ResetPasswordEndpoint
{
    public static async Task<IResult> Handle(
        IAuthService authService,
        ResetPasswordRequestDto request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        Result<string> result = await authService.ResetPasswordAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Password reset initiated for {Email}", request.Email);

            return Ok(new { message = result.Value });
        }

        return Json(new ProblemDetails
        {
            Status = 404,
            Title = "Not Found",
            Detail = result.Error,
            Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
        }, statusCode: 404);
    }
}
