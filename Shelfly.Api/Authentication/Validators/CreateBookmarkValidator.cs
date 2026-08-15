using FluentValidation;
using Shelfly.Api.Models;

namespace Shelfly.Api.Authentication.Validators;

public class CreateBookmarkValidator : AbstractValidator<CreateBookmarkRequest>
{
    public CreateBookmarkValidator()
    {
        RuleFor(r => r.StartPage)
            .GreaterThan(0).WithMessage("Start page must be greater than 0");

        RuleFor(r => r.EndPage)
            .GreaterThanOrEqualTo(r => r.StartPage).When(r => r.EndPage.HasValue)
            .WithMessage("End page must be greater than or equal to start page");

        RuleFor(r => r.Note)
            .MaximumLength(1000).When(r => r.Note is not null)
            .WithMessage("Note must not exceed 1000 characters");
    }
}
