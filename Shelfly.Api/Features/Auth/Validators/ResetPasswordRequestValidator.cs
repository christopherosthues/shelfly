using FluentValidation;
using Shelfly.Api.Features.Auth.DTOs;

namespace Shelfly.Api.Features.Auth.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}
