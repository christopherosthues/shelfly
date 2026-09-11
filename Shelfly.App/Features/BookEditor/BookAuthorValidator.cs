using Shelfly.App.Controls;
using Shelfly.App.Resources.Localization;
using Shelfly.Common;

namespace Shelfly.App.Features.BookEditor;

public class BookAuthorValidator : IValidator<string>
{
    public const int MaxLength = 256;

    public Result<bool> Validate(string value, object? parameter = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<bool>.Failure(AppResources.BookEditPageAuthorEmptyError);
        }

        if (value.Length > MaxLength)
        {
            return Result<bool>.Failure(AppResources.BookEditPageAuthorMaxLengthError);
        }

        return Result<bool>.Success(true);
    }

    public Result<bool> Validate(object value, object? parameter = null)
    {
        if (value is not string text)
        {
            return Result<bool>.Failure(AppResources.BookEditPageAuthorEmptyError);
        }

        return Validate(text);
    }
}
