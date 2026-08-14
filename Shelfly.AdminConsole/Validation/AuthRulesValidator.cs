using FluentValidation;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Validation;

public class AuthRulesValidator : AbstractValidator<AuthorizationRule>
{
    public AuthRulesValidator()
    {
        RuleFor(x => x.Rules)
            .NotEmpty().WithMessage("Rules array must contain at least one rule")
            .Must(rules => rules.DistinctBy(r => r.EndpointPattern).Count() == rules.Count)
            .WithMessage("Endpoint patterns must be unique across all rules");

        RuleForEach(x => x.Rules).SetValidator(new RuleValidator());
    }
}

public class RuleValidator : AbstractValidator<Rule>
{
    public RuleValidator()
    {
        RuleFor(r => r.EndpointPattern)
            .NotEmpty().WithMessage("EndpointPattern is required")
            .Must(pattern => pattern.Contains(':'))
            .WithMessage("EndpointPattern must match format <METHOD>:<PATH>");

        RuleFor(r => r.RequiredRoles)
            .NotEmpty().WithMessage("RequiredRoles must contain at least one role")
            .Must(roles => roles.Distinct().Count() == roles.Count)
            .WithMessage("RequiredRoles should not contain duplicates within a single rule");
    }
}
