using FluentValidation;
using Shelfly.Api.Models;

namespace Shelfly.Api.Authentication.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookValidator()
    {
        RuleFor(r => r.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(256).WithMessage("Title must not exceed 256 characters");

        RuleFor(r => r.ISBN)
            .MaximumLength(16).WithMessage("ISBN must not exceed 16 characters");

        RuleFor(r => r.PublishDate)
            .LessThan(DateTime.Now).WithMessage("Publish date must be in the past")
            .GreaterThanOrEqualTo(new DateTime(1800, 1, 1)).WithMessage("Publish date seems too early");
    }
}
