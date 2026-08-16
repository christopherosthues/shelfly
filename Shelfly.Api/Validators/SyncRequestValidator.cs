using FluentValidation;
using Shelfly.Common.DTOs;

namespace Shelfly.Api.Validators;

public class SyncUploadRequestValidator : AbstractValidator<SyncUploadRequest>
{
    public SyncUploadRequestValidator()
    {
        RuleFor(r => r.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .Must(e => e == "Book" || e == "Bookmark").WithMessage("Entity type must be 'Book' or 'Bookmark'");

        RuleForEach(r => r.Items).SetInheritanceValidator(v =>
        {
            v.Add(new SyncItemValidator());
        });
    }
}

public class SyncItemValidator : AbstractValidator<SyncItem>
{
    public SyncItemValidator()
    {
        RuleFor(i => i.LocalGuid)
            .NotEmpty().WithMessage("Local GUID is required");

        RuleFor(i => i.Title)
            .MaximumLength(256).When(x => x.Title != null).WithMessage("Title must not exceed 256 characters");

        RuleFor(i => i.Isbn)
            .MaximumLength(16).When(x => x.Isbn != null).WithMessage("ISBN must not exceed 16 characters");

        RuleFor(i => i.LastModified)
            .NotEmpty().WithMessage("Last modified timestamp is required");
    }
}

public class SyncDownloadRequestValidator : AbstractValidator<SyncDownloadRequest>
{
    public SyncDownloadRequestValidator()
    {
        RuleFor(r => r.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .Must(e => e == "Book" || e == "Bookmark").WithMessage("Entity type must be 'Book' or 'Bookmark'");

        RuleForEach(r => r.LocalGuids).SetInheritanceValidator(v =>
        {
            v.Add(new SyncLocalGuidValidator());
        });
    }
}

public class SyncLocalGuidValidator : AbstractValidator<Guid>
{
    public SyncLocalGuidValidator()
    {
        RuleFor(g => g)
            .NotEmpty().WithMessage("Local GUID cannot be empty");
    }
}

public class SyncConflictResolutionRequestValidator : AbstractValidator<SyncConflictResolutionRequest>
{
    public SyncConflictResolutionRequestValidator()
    {
        RuleFor(r => r.LocalGuid)
            .NotEmpty().WithMessage("Local GUID is required");

        RuleFor(r => r.RemoteGuid)
            .NotEmpty().WithMessage("Remote GUID is required");

        RuleFor(r => r.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .Must(e => e == "Book" || e == "Bookmark").WithMessage("Entity type must be 'Book' or 'Bookmark'");

        RuleFor(r => r.Resolution)
            .NotEmpty().WithMessage("Resolution is required")
            .Must(res => res == "LocalWins" || res == "RemoteWins").WithMessage("Resolution must be 'LocalWins' or 'RemoteWins'");
    }
}
