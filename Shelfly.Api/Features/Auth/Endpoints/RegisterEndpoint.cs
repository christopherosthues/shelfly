using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Api.Features.Auth.Services;
using Shelfly.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Auth.Endpoints;

public static class RegisterEndpoint
{
    public static async Task<IResult> Handle(
        IAuthService authService,
        RegisterRequestDto request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        Result<AuthResponseDto> result = await authService.RegisterAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogInformation("User registered: {Email} with ID {UserId}", request.Email, result.Value.UserId);

            return Created("", result.Value);
        }

        return Conflict(new ProblemDetails
        {
            Status = 409,
            Title = "Conflict",
            Detail = result.Error,
            Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
        });
    }
}
