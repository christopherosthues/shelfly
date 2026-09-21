using Shelfly.Api.Features.Auth.DTOs;
using Shelfly.Common;

namespace Shelfly.Api.Features.Auth.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
    Task<Result<AuthResponseDto>> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken);
    Task<Result<string>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken);
}
