using FluentValidation;
using Shelfly.Api.Authentication.Models;

namespace Shelfly.Api.Authentication.Validators;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("Valid email address required");

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}