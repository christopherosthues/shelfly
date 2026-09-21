using FluentValidation;
using Shelfly.Api.Features.Auth.DTOs;

namespace Shelfly.Api.Features.Auth.Validators;

public class RefreshRequestValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
