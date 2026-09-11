using Shelfly.App.Controls;
using Shelfly.App.Resources.Localization;
using Shelfly.Common;

namespace Shelfly.App.Features.BookEditor;

public class BookTitleValidator : IValidator<string>
{
    public const int MaxLength = 256;

    public Result<bool> Validate(string value, object? parameter = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<bool>.Failure(AppResources.BookEditPageTitleEmptyError);
        }

        if (value.Length > MaxLength)
        {
            return Result<bool>.Failure(AppResources.BookEditPageTitleMaxLengthError);
        }

        return Result<bool>.Success(true);
    }

    public Result<bool> Validate(object value, object? parameter = null)
    {
        if (value is not string text)
        {
            return Result<bool>.Failure(AppResources.BookEditPageTitleEmptyError);
        }

        return Validate(text);
    }
}
