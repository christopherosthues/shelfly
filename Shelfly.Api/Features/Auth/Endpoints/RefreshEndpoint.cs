using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Api.Features.Auth.Services;
using Shelfly.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Auth.Endpoints;

public static class RefreshEndpoint
{
    public static async Task<IResult> Handle(
        IAuthService authService,
        RefreshRequestDto request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        Result<AuthResponseDto> result = await authService.RefreshAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("Token refreshed successfully");

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
