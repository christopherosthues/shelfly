using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Common;

namespace Shelfly.Api.Features.Auth.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
}
