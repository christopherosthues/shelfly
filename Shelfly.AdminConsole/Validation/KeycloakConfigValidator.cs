using FluentValidation;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Validation;

public class KeycloakConfigValidator : AbstractValidator<KeycloakConfiguration>
{
    public KeycloakConfigValidator()
    {
        RuleFor(x => x.IssuerUrl)
            .NotEmpty().WithMessage("IssuerUrl is required")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("IssuerUrl must be a valid URL");

        RuleFor(x => x.Audience)
            .NotEmpty().WithMessage("Audience is required");

        RuleFor(x => x.JwksEndpoint)
            .NotEmpty().WithMessage("JwksEndpoint is required")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("JwksEndpoint must be a valid URL");
    }
}
