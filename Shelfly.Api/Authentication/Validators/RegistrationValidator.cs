using FluentValidation;
using Shelfly.Api.Authentication.Models;

namespace Shelfly.Api.Authentication.Validators;

public class RegistrationValidator : AbstractValidator<RegistrationRequest>
{
    public RegistrationValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("Valid email address required");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}