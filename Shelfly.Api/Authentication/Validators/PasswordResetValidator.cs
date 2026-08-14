using FluentValidation;
using Shelfly.Api.Authentication.Models;

namespace Shelfly.Api.Authentication.Validators;

public class PasswordResetValidator : AbstractValidator<PasswordResetRequest>
{
    public PasswordResetValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("Valid email address required");
    }
}