using FluentValidation;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Validation;

public class PostgreSQLConfigValidator : AbstractValidator<PostgreSQLConfiguration>
{
    public PostgreSQLConfigValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("ConnectionString is required");
    }
}
