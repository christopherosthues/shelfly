using FluentValidation;

namespace Shelfly.Api.Features.Books.Validators;

public class BookStatusUpdateRequest
{
    // TODO: deletedAt datetime instead of string
    public string Status { get; set; } = string.Empty;
}

public class BookStatusUpdateValidator : AbstractValidator<BookStatusUpdateRequest>
{
    public BookStatusUpdateValidator()
    {
        RuleFor(request => request.Status)
            .Must(status => status == "Active" || status == "SoftDeleted")
            .WithMessage("Status must be Active or SoftDeleted");
    }
}
