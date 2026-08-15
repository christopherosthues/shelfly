using FluentValidation;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Validation;

public class PostgreSqlConfigValidator : AbstractValidator<PostgreSqlConfiguration>
{
    public PostgreSqlConfigValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("ConnectionString is required");
    }
}
