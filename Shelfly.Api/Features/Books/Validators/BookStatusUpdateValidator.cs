using FluentValidation;
using Shelfly.Common.Enums;

namespace Shelfly.Api.Features.Books.Validators;

public class BookStatusUpdateRequest
{
    public DeletionStatus Status { get; set; }
}

public class BookStatusUpdateValidator : AbstractValidator<BookStatusUpdateRequest>
{
    public BookStatusUpdateValidator()
    {
        RuleFor(request => request.Status)
            .Must(status => status == DeletionStatus.Active || status == DeletionStatus.SoftDeleted)
            .WithMessage("Status must be Active or SoftDeleted");
    }
}
