using FluentValidation;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Validation;

public class PostgreSqlConfigValidator : AbstractValidator<PostgreSqlConfiguration>
{
    public PostgreSqlConfigValidator()
    {
        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("ConnectionString is required");
    }
}
