using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Api.Features.Auth.Services;
using Shelfly.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Auth.Endpoints;

public static class LoginEndpoint
{
    public static async Task<IResult> Handle(
        IAuthService authService,
        LoginRequestDto request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        Result<AuthResponseDto> result = await authService.LoginAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("User logged in: {Email}", request.Email);

            return Ok(result.Value);
        }

        return Json(new ProblemDetails
        {
            Status = 401,
            Title = "Unauthorized",
            Detail = result.Error,
            Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
        }, statusCode: 401);
    }
}
